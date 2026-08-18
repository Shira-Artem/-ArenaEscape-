using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Чувствительность")]
    public float sensitivity = 200f;
    public float smoothing = 0f;

    float _pitch, _targetPitch, _targetYaw, _currentYaw;
    float _recoilPitch, _recoilYaw;
    Transform _body;
    PlayerHealth _ph;
    bool _ready;

    void Start()
    {
        _body = transform.parent;
        if (_body == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _body = p.transform;
        }

        if (_body != null)
        {
            _currentYaw = _body.eulerAngles.y;
            _ph = _body.GetComponent<PlayerHealth>();
        }
        _ready = _body != null;
        Lock();
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            Unlock();
            return;
        }

        if (_body == null)
        {
            _ready = false;
            return;
        }

        if (_ph != null && _ph.isDead)
        {
            Unlock();
            return;
        }

        if (UiOpen)
        {
            Unlock();
            return;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked
            && (GameManager.Instance == null || !GameManager.Instance.IsGameWon))
            Lock();

        if (!_ready) return;

        const float MOUSE_SCALE = 1f / 60f;
        float mx = Input.GetAxisRaw("Mouse X") * sensitivity * MOUSE_SCALE;
        float my = Input.GetAxisRaw("Mouse Y") * sensitivity * MOUSE_SCALE;

        // Защита от аномальных дельт мыши (перехват фокуса, alt-tab, лаг)
        if (Mathf.Abs(mx) > 30f) mx = 0f;
        if (Mathf.Abs(my) > 30f) my = 0f;

        _targetPitch = Mathf.Clamp(_targetPitch - my, -80f, 80f);
        _targetYaw += mx;

        if (smoothing <= 0.01f)
        {
            _pitch      = _targetPitch + _recoilPitch;
            _currentYaw = _targetYaw   + _recoilYaw;
        }
        else
        {
            float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
            _pitch      = Mathf.Lerp(_pitch,      _targetPitch + _recoilPitch, t);
            _currentYaw = Mathf.Lerp(_currentYaw, _targetYaw   + _recoilYaw,   t);
        }

        _recoilPitch = Mathf.Lerp(_recoilPitch, 0f, 10f * Time.deltaTime);
        _recoilYaw = Mathf.Lerp(_recoilYaw, 0f, 10f * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(_pitch, 0, 0);
        if (_body != null)
            _body.rotation = Quaternion.Euler(0, _currentYaw, 0);
    }

    public static bool UiOpen;

    public static void Lock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static void Unlock()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ApplyRecoil(float pitchUp, float yawVariance)
    {
        _recoilPitch += pitchUp;
        _recoilYaw += Random.Range(-yawVariance, yawVariance);
    }
}
