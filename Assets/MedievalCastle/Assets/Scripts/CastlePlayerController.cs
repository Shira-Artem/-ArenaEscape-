using UnityEngine;

/// <summary>
/// Контроллер игрока для сцены замка.
/// Старый учебный HUD выключен по умолчанию, чтобы не мешать сюжетному прологу.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CastlePlayerController : MonoBehaviour
{
    [Header("Движение")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 11f;
    public float jumpForce = 5.5f;

    [Header("Физика")]
    public float groundCheckRadius = 0.3f;
    public LayerMask groundMask = ~0;

    [Header("Debug")]
    public bool showLegacyCastleHud = false;

    private Rigidbody rb;
    private CapsuleCollider col;
    private bool grounded;

    public static string CurrentZone = "Ворота замка";

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.mass = 80f;
        rb.linearDamping = 6f;
        rb.angularDamping = 999f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        col.height = 1.8f;
        col.radius = 0.35f;
        col.center = new Vector3(0f, 0.9f, 0f);

        gameObject.tag = "Player";

        LockCursor();
    }

    private void Update()
    {
        CheckCursorInput();
        HandleJump();
        UpdateZoneLabel();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        HandleMovement();
    }

    private void HandleMovement()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += 1f;

        if (h == 0f && v == 0f)
        {
            Vector3 vel = rb.linearVelocity;
            vel.x = Mathf.Lerp(vel.x, 0f, 0.25f);
            vel.z = Mathf.Lerp(vel.z, 0f, 0.25f);
            rb.linearVelocity = vel;
            return;
        }

        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        Vector3 dir = (transform.forward * v + transform.right * h).normalized;

        Vector3 targetVel = dir * speed;
        targetVel.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVel;
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;
        }
    }

    private void CheckGrounded()
    {
        Vector3 feetPos = transform.position + Vector3.down * 0.05f;
        Collider[] hits = Physics.OverlapSphere(feetPos, groundCheckRadius, groundMask);

        grounded = false;

        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject)
            {
                grounded = true;
                break;
            }
        }
    }

    private void CheckCursorInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UpdateZoneLabel()
    {
        Vector3 p = transform.position;

        if (p.z < -15f)
            CurrentZone = "Ворота замка";
        else if (p.z < -2f)
            CurrentZone = "Внешний двор";
        else if (p.z < 9f)
            CurrentZone = "Внутренний двор";
        else if (p.x >= 35f)
            CurrentZone = "Тронный зал";
        else if (p.y < -2f)
            CurrentZone = "Подземелье";
        else
            CurrentZone = "Замок";
    }

    private void OnGUI()
    {
        if (!showLegacyCastleHud)
            return;

        GUI.color = new Color(0f, 0f, 0f, 0.62f);
        GUI.Box(new Rect(8, 8, 265, 130), "");
        GUI.color = Color.white;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };
        titleStyle.normal.textColor = new Color(1f, 0.85f, 0.25f);

        GUI.Label(new Rect(16, 14, 250, 24), "СРЕДНЕВЕКОВЫЙ ЗАМОК", titleStyle);

        GUIStyle textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11
        };
        textStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(16, 42, 250, 85),
            "Зона: " + CurrentZone + "\n" +
            "WASD — движение\n" +
            "Shift — бег\n" +
            "Пробел — прыжок\n" +
            "Esc — курсор", textStyle);
    }
}
