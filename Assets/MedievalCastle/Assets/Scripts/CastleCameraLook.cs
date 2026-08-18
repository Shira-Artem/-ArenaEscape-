using UnityEngine;

/// <summary>
/// Камера от первого лица — Средневековый замок.
/// Более надёжная версия:
/// - yaw хранится отдельно, поэтому вращение влево/вправо стабильнее;
/// - используется LateUpdate, чтобы камера не дёргалась после физики;
/// - сохраняется ограничение по pitch.
/// </summary>
public class CastleCameraLook : MonoBehaviour
{
    public Transform player;
    public float sensitivity = 180f;
    public float minPitch = -75f;
    public float maxPitch = 75f;

    private float pitch;
    private float yaw;

    private void Start()
    {
        pitch = NormalizeAngle(transform.localEulerAngles.x);
        if (player != null)
            yaw = NormalizeAngle(player.eulerAngles.y);
    }

    private void LateUpdate()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        float rawX = Input.GetAxisRaw("Mouse X");
        float rawY = Input.GetAxisRaw("Mouse Y");

        // Fallback на обычные оси, если Raw слишком мал.
        if (Mathf.Abs(rawX) < 0.0001f) rawX = Input.GetAxis("Mouse X");
        if (Mathf.Abs(rawY) < 0.0001f) rawY = Input.GetAxis("Mouse Y");

        float mouseX = rawX * sensitivity * Time.deltaTime;
        float mouseY = rawY * sensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (player != null)
            player.rotation = Quaternion.Euler(0f, yaw, 0f);

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        return angle;
    }
}
