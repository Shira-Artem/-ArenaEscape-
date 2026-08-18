using UnityEngine;

public class L0GateBlocker : MonoBehaviour
{
    public Vector3 size = new Vector3(5.2f, 4.3f, 0.36f);
    public Color woodColor = new Color(0.28f, 0.12f, 0.04f);
    public Color metalColor = new Color(0.12f, 0.12f, 0.14f);

    private GameObject visualRoot;
    private Collider blockerCollider;

    private void Awake()
    {
        Build();
    }

    public void Build()
    {
        ClearChildren();

        transform.localScale = Vector3.one;

        blockerCollider = GetComponent<Collider>();
        if (blockerCollider == null)
            blockerCollider = gameObject.AddComponent<BoxCollider>();

        blockerCollider.isTrigger = false;

        BoxCollider box = blockerCollider as BoxCollider;
        if (box != null)
        {
            box.center = Vector3.zero;
            box.size = size;
        }

        visualRoot = new GameObject("GateBlocker_Visual");
        visualRoot.transform.SetParent(transform);
        visualRoot.transform.localPosition = Vector3.zero;
        visualRoot.transform.localRotation = Quaternion.identity;

        int bars = 7;
        for (int i = 0; i < bars; i++)
        {
            float x = Mathf.Lerp(-size.x * 0.42f, size.x * 0.42f, i / (float)(bars - 1));
            CreateBar("Vertical_" + i, new Vector3(x, 0f, 0f), new Vector3(0.16f, size.y, 0.16f), woodColor);
        }

        CreateBar("Cross_A", new Vector3(0f, 0.85f, 0.02f), new Vector3(size.x, 0.18f, 0.18f), woodColor);
        CreateBar("Cross_B", new Vector3(0f, -0.75f, 0.02f), new Vector3(size.x, 0.18f, 0.18f), woodColor);
        CreateBar("Metal_Lock", new Vector3(0f, 0f, -0.08f), new Vector3(1.0f, 0.55f, 0.10f), metalColor);
    }

    public void SetLocked(bool locked)
    {
        if (blockerCollider != null)
            blockerCollider.enabled = locked;

        if (visualRoot != null)
            visualRoot.SetActive(locked);
    }

    private void CreateBar(string name, Vector3 localPosition, Vector3 localScale, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(visualRoot.transform);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = localScale;

        Collider col = obj.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            r.material = new Material(Shader.Find("Standard"));
            r.material.color = color;
        }
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}
