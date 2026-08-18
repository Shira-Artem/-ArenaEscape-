using UnityEngine;

/// <summary>
/// Короткий billboard-компонент для редких важных табличек L0Layout.
/// v4: текст меньше и исчезает раньше, чтобы не висел огромной надписью в кадре.
/// </summary>
public class L0Label : MonoBehaviour
{
    public float visibleDistance = 10.5f;
    public bool fadeByDistance = true;

    private TextMesh textMesh;
    private Transform player;

    private void Awake()
    {
        textMesh = GetComponent<TextMesh>();
    }

    private void Start()
    {
        FindPlayer();
    }

    private void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 direction = transform.position - cam.transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        if (!fadeByDistance || textMesh == null) return;

        if (player == null) FindPlayer();
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        float alpha = Mathf.Clamp01(1f - distance / Mathf.Max(0.01f, visibleDistance));

        Color color = textMesh.color;
        color.a = alpha;
        textMesh.color = color;
    }

    private void FindPlayer()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null) player = obj.transform;
    }
}
