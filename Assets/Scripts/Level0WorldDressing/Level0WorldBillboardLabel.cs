using UnityEngine;

/// <summary>
/// Компонент для текстовых меток, которые всегда смотрят на камеру.
/// Может затухать по расстоянию.
/// </summary>
public class Level0WorldBillboardLabel : MonoBehaviour
{
    public float visibleDistance = 14f;
    public bool fadeByDistance = true;
    public float characterSize = 0.12f;

    private TextMesh textMesh;
    private Transform player;

    private void Start()
    {
        textMesh = GetComponent<TextMesh>();
        if (textMesh != null) textMesh.characterSize = characterSize;
        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void LateUpdate()
    {
        if (Camera.main == null) return;
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);

        if (!fadeByDistance || textMesh == null) return;

        if (player == null) FindPlayer();
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        float alpha = Mathf.Clamp01(1f - (dist / visibleDistance));
        Color c = textMesh.color;
        c.a = alpha;
        textMesh.color = c;
    }
}
