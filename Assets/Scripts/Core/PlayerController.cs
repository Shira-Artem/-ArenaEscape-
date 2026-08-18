using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Speed")]
    public float walkSpeed = 6.8f;
    public float runSpeed  = 12.0f;

    [Header("Feel")]
    public float acceleration = 55f;
    public float deceleration = 40f;
    public float airControl   = 0.45f;

    [Header("Jump")]
    public float jumpForce      = 8.5f;
    public float gravity        = -36f;
    public float fallGravityMult = 2.5f;
    public float coyoteTime     = 0.16f;
    public float jumpBufferTime = 0.18f;

    [Header("Dodge")]
    public float dodgeForce    = 15f;
    public float dodgeDuration = 0.22f;
    public float dodgeCooldown = 0.85f;
    public float dodgeFovPunch = 10f;

    [Header("Ground")]
    public Transform groundCheck;
    public float     groundRadius = 0.22f;
    public LayerMask groundMask;
    public bool      useCharacterControllerGrounded = false;

    [Header("Camera")]
    public bool  headBobEnabled = false;
    public float bobFrequency  = 7f;
    public float bobAmplitude  = 0.035f;
    public float bobSmoothing  = 12f;

    CharacterController _cc;
    Vector3 _horizontalVelocity;
    float   _verticalVelocity;
    float   _coyoteTimer;
    float   _bufferTimer;

    // Jump: 0=can ground jump, 1=used ground jump (can double), 2=all spent
    int  _jumpCount;
    bool _wasGrounded;
    int  _groundedFrames;
    float _peakY;
    float _jumpLockout;
    float _airTimer;

    // Dodge state
    float   _dodgeTimer;
    float   _dodgeCooldownTimer;
    Vector3 _dodgeDir;
    float   _dodgeFovOffset;
    float   _afterimageTimer;

    // Camera
    Transform _camTransform;
    Camera    _cam;
    Vector3   _camOrigin;
    float     _bobTimer;
    Vector3   _bobOffset;
    float     _baseFov;

    public static float DodgeFlashTimer;

    public bool  IsDodging              => _dodgeTimer > 0f;
    public float DodgeCooldownProgress  => dodgeCooldown > 0f
        ? Mathf.Clamp01(1f - _dodgeCooldownTimer / dodgeCooldown)
        : 1f;

    void Awake() { Instance = this; }

    void Start()
    {
        _cc = GetComponent<CharacterController>();

        if (groundCheck == null)
        {
            var gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            groundCheck = gc.transform;
        }

        if (groundMask.value == 0)
        {
            int ign = LayerMask.NameToLayer("Ignore Raycast");
            groundMask = ign >= 0 ? ~(1 << ign) : ~0;
        }

        _cam = GetComponentInChildren<Camera>();
        if (_cam != null)
        {
            _camTransform = _cam.transform;
            _camOrigin    = _camTransform.localPosition;
            _baseFov      = _cam.fieldOfView;
        }

        Physics.gravity = new Vector3(0f, -36f, 0f);
        _cc.slopeLimit = 52f;
        _cc.stepOffset = 0.45f;
        _jumpCount      = 0;
        _wasGrounded    = true;
        _groundedFrames = 10;
        _peakY          = transform.position.y;
    }

    void Update()
    {
        if (Time.timeScale == 0f || _cc == null || !_cc.enabled) return;

        float dt = Time.deltaTime;
        _jumpLockout -= dt;
        bool grounded = IsGrounded();

        // ─── AIR TIMER (tracks continuous time since last jump) ───
        if (_jumpCount > 0)
            _airTimer += dt;

        // ─── GROUND / LANDING ───
        // Bulletproof reset: only when physics says grounded, we are FALLING,
        // AND we've been airborne long enough that this isn't a ground-detection flicker.
        bool realGrounded = grounded
            && _verticalVelocity < -2f
            && _jumpLockout <= 0f
            && (_jumpCount == 0 || _airTimer > 0.45f);

        if (realGrounded)
        {
            _groundedFrames++;

            if (_groundedFrames >= 3 && _jumpCount != 0)
            {
                float fallH = _peakY - transform.position.y;
                if (fallH > 3f)
                {
                    float mag = Mathf.Clamp(0.08f + (fallH - 3f) * 0.02f, 0.08f, 0.28f);
                    Camera.main?.GetComponent<CameraShake>()?.Shake(mag, 0.18f);
                }
                _jumpCount = 0;
                _airTimer  = 0f;
            }

            if (_jumpCount == 0)
                _airTimer = 0f;

            _peakY = transform.position.y;

            if (_verticalVelocity < 0f)
                _verticalVelocity = -3f;
        }
        else
        {
            _groundedFrames = 0;
            _peakY = Mathf.Max(_peakY, transform.position.y);
        }

        // ─── COYOTE TIME ───
        if (realGrounded && _jumpCount == 0)
        {
            _coyoteTimer = coyoteTime;
        }
        else if (!realGrounded && _jumpCount == 0)
        {
            _coyoteTimer -= dt;
            if (_coyoteTimer <= 0f)
                _jumpCount = 1;
        }

        // ─── INPUT ───
        Vector3 input = Vector3.ClampMagnitude(
            new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")), 1f);
        bool hasMovementInput = input.sqrMagnitude > 0.1f;

        bool spaceDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);

        // ─── DODGE (Q / E) ───
        bool dodgeQ = Input.GetKeyDown(KeyCode.Q);
        bool dodgeE = Input.GetKeyDown(KeyCode.E);

        bool wantDodge = (dodgeQ || dodgeE)
                         && _dodgeCooldownTimer <= 0f
                         && _dodgeTimer <= 0f;

        if (wantDodge)
        {
            float sideSign = dodgeQ ? -1f : 1f;
            if (hasMovementInput)
            {
                Vector3 moveDir = (transform.right * input.x + transform.forward * input.z).normalized;
                _dodgeDir = (moveDir + transform.right * sideSign * 0.4f).normalized;
            }
            else
            {
                _dodgeDir = transform.right * sideSign;
            }

            _dodgeTimer         = dodgeDuration;
            _dodgeCooldownTimer = dodgeCooldown;
            _dodgeFovOffset     = dodgeFovPunch;
            _afterimageTimer    = 0f;

            GetComponent<PlayerHealth>()?.SetInvincible(dodgeDuration + 0.06f);
            FeedbackManager.Instance?.PlayWhoosh();
            DodgeFlashTimer = 0.18f;
            GameAudioManager.Instance?.Play(GameAudioManager.SFX.Dodge, 0.45f);
            Camera.main?.GetComponent<CameraShake>()?.Shake(0.14f, 0.16f);

            // Directional particle burst
            Color coreCol = dodgeQ
                ? new Color(0.25f, 0.55f, 1f, 0.7f)
                : new Color(1f, 0.55f, 0.25f, 0.7f);
            Color fadeCol = dodgeQ
                ? new Color(0.1f, 0.25f, 0.8f, 0.25f)
                : new Color(0.8f, 0.25f, 0.1f, 0.25f);
            FeedbackManager.Instance?.SpawnBurst(
                transform.position + Vector3.up * 0.35f - _dodgeDir * 0.6f,
                coreCol, fadeCol, 10, 3.2f, 0.09f);
            SpawnAfterimage(0.85f);
        }

        // ─── JUMP BUFFER ───
        if (spaceDown)
            _bufferTimer = jumpBufferTime;

        // ─── TIMERS ───
        _bufferTimer        -= dt;
        _dodgeCooldownTimer -= dt;
        if (_dodgeTimer > 0f)
        {
            _dodgeTimer -= dt;

            // Spawn trail of afterimages during dodge
            _afterimageTimer -= dt;
            if (_afterimageTimer <= 0f && _dodgeTimer > 0.04f)
            {
                SpawnAfterimage(0.55f);
                _afterimageTimer = 0.06f;
            }
        }

        // ─── HORIZONTAL MOVEMENT ───
        if (_dodgeTimer > 0f)
        {
            float t = Mathf.Clamp01(_dodgeTimer / dodgeDuration);
            _horizontalVelocity = _dodgeDir * (dodgeForce * (t * t));
        }
        else
        {
            float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            Vector3 targetVel = (transform.right * input.x + transform.forward * input.z) * targetSpeed;
            float control     = grounded ? 1f : airControl;
            float accel       = input.sqrMagnitude > 0.01f ? acceleration : deceleration;
            _horizontalVelocity = Vector3.MoveTowards(
                _horizontalVelocity, targetVel, accel * control * dt);
        }

        // ─── JUMP EXECUTION ───
        if (_bufferTimer > 0f && _dodgeTimer <= 0f)
        {
            if (_jumpCount == 0 && _coyoteTimer > 0f)
            {
                _verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
                _bufferTimer    = 0f;
                _coyoteTimer    = 0f;
                _jumpCount      = 1;
                _jumpLockout    = 0.35f;
                _groundedFrames = 0;
                _airTimer       = 0f;
            }
            else if (_jumpCount == 1 && !realGrounded)
            {
                _verticalVelocity = Mathf.Sqrt(jumpForce * 0.85f * -2f * gravity);
                _bufferTimer = 0f;
                _jumpCount   = 2;
                _jumpLockout = 0.35f;
                _airTimer    = 0f;

                GameManager.Instance?.ShowMessage("⬆ ДВОЙНОЙ ПРЫЖОК", 0.4f);
                FeedbackManager.Instance?.SpawnBurst(
                    transform.position + Vector3.up * 0.3f,
                    new Color(1f, 0.92f, 0.15f, 0.8f),
                    new Color(0.9f, 0.70f, 0.05f, 0.3f), 8, 2.5f, 0.07f);
            }
            // jumpCount == 2 → nothing happens (all jumps spent)
        }

        // ─── GRAVITY ───
        float grav = _verticalVelocity < 0f ? gravity * fallGravityMult : gravity;
        _verticalVelocity += grav * dt;
        _cc.Move((_horizontalVelocity + Vector3.up * _verticalVelocity) * dt);

        // ─── CAMERA EFFECTS ───
        if (_camTransform != null)
        {
            float tiltTarget = _dodgeTimer > 0f
                ? Vector3.Dot(_dodgeDir, transform.right) * -16f
                : 0f;
            var euler = _camTransform.localRotation.eulerAngles;
            float newZ = Mathf.LerpAngle(euler.z, tiltTarget, 16f * dt);
            _camTransform.localRotation = Quaternion.Euler(euler.x, euler.y, newZ);
        }

        if (_cam != null)
        {
            _dodgeFovOffset = Mathf.MoveTowards(_dodgeFovOffset, 0f, dodgeFovPunch * 3.5f * dt);
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, _baseFov + _dodgeFovOffset, 12f * dt);
        }

        _wasGrounded = realGrounded;
        UpdateHeadBob(grounded, input.magnitude);
    }

    void SpawnAfterimage(float alpha)
    {
        var ghost = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        ghost.name = "DodgeGhost";
        ghost.transform.position = transform.position + Vector3.up * 0.9f;
        ghost.transform.rotation = transform.rotation;
        ghost.transform.localScale = new Vector3(0.5f, 0.85f, 0.5f);
        Object.Destroy(ghost.GetComponent<Collider>());

        var mr = ghost.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Diffuse"));
            mat.color = new Color(0.35f, 0.55f, 1f, alpha * 0.4f);
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetFloat("_SrcBlend", 5f);
                mat.SetFloat("_DstBlend", 10f);
                mat.SetFloat("_ZWrite", 0f);
                mat.renderQueue = 3000;
            }
            mr.material = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        Object.Destroy(ghost, 0.22f);
    }

    bool IsGrounded()
    {
        bool cc    = useCharacterControllerGrounded && _cc != null && _cc.isGrounded;
        bool sphere = groundCheck != null &&
            Physics.CheckSphere(groundCheck.position, groundRadius, groundMask,
                QueryTriggerInteraction.Ignore);
        return cc || sphere;
    }

    void UpdateHeadBob(bool grounded, float inputMag)
    {
        if (_camTransform == null) return;

        if (!headBobEnabled)
        {
            _camTransform.localPosition = Vector3.Lerp(
                _camTransform.localPosition, _camOrigin, 14f * Time.deltaTime);
            return;
        }

        float targetBob = 0f;
        if (grounded && inputMag > 0.1f)
        {
            float runMult = Input.GetKey(KeyCode.LeftShift) ? 1.35f : 1f;
            _bobTimer += Time.deltaTime * bobFrequency * runMult;
            targetBob  = Mathf.Sin(_bobTimer) * bobAmplitude;
        }
        else _bobTimer = 0f;

        _bobOffset = Vector3.Lerp(_bobOffset, new Vector3(0f, targetBob, 0f), bobSmoothing * Time.deltaTime);
        _camTransform.localPosition = _camOrigin + _bobOffset;
    }

    public void BounceUp(float force)
    {
        _verticalVelocity = force;
    }

    public void AddLunge(Vector3 dir, float force)
    {
        if (_dodgeTimer > 0f) return;
        var h = new Vector3(dir.x, 0f, dir.z).normalized;
        _horizontalVelocity += h * force;
        float cap = runSpeed * 1.8f;
        if (_horizontalVelocity.magnitude > cap)
            _horizontalVelocity = _horizontalVelocity.normalized * cap;
    }
}
