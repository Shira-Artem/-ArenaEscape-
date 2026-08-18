using UnityEngine;

/// <summary>
/// Low-level primitive helpers for the modular orc arena. Authored combat props
/// belong to L0OrcArenaCombatPropsModule.
/// </summary>
public static class L0OrcArenaPrimitiveKit
{
    public static GameObject CreateGroup(string name, Transform parent)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(parent, true);
        return group;
    }

    public static GameObject CreateGroundPatchWalkable(Transform parent, string name, Vector3 position, Quaternion rotation, Vector3 scale, Color color)
    {
        GameObject root = CreateGroup(name, parent);
        root.transform.position = position;
        root.transform.rotation = rotation;
        CreatePrimitive(root.transform, "Patch", PrimitiveType.Cube, Vector3.zero, Quaternion.identity, scale, color, true);
        return root;
    }

    public static GameObject CreateGroundPatchDecorative(Transform parent, string name, Vector3 position, Quaternion rotation, Vector3 scale, Color color, bool emissive = false)
    {
        GameObject root = CreateGroup(name, parent);
        root.transform.position = position;
        root.transform.rotation = rotation;
        CreatePrimitive(root.transform, "Patch_NoCollider", PrimitiveType.Cube, Vector3.zero, Quaternion.identity, scale, color, false, emissive);
        return root;
    }

    public static GameObject CreateWalkablePlaneCollider(Transform parent, string name, Vector3 position, Quaternion rotation, Vector2 size)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        obj.transform.localRotation = rotation;
        obj.transform.localScale = new Vector3(size.x / 10f, 1f, size.y / 10f);

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = false;

        return obj;
    }

    public static GameObject CreatePrimitive(Transform parent, string name, PrimitiveType type, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, Color color, bool keepCollider, bool emissive = false, bool transparent = false)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPosition;
        obj.transform.localRotation = localRotation;
        obj.transform.localScale = localScale;

        if (!keepCollider)
            RemoveCollider(obj);

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = L0OrcArenaMaterials.Create(color, emissive, transparent);

        return obj;
    }

    public static void RemoveCollider(GameObject obj)
    {
        if (obj == null)
            return;

        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
            RemoveCollider(collider);
    }

    public static void RemoveCollider(Collider collider)
    {
        if (collider == null)
            return;

        if (Application.isPlaying) UnityEngine.Object.Destroy(collider);
        else UnityEngine.Object.DestroyImmediate(collider);
    }

    public static void ClearTallCollidersInCorridor(Transform root, Vector3 start, Vector3 end, float halfWidth, float maxFloorHeight)
    {
        if (root == null)
            return;

        Collider[] colliders = root.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null)
                continue;

            Bounds bounds = collider.bounds;
            if (bounds.max.y <= maxFloorHeight)
                continue;

            float horizontalExtent = Mathf.Max(bounds.extents.x, bounds.extents.z);
            if (L0OrcArenaConfig.DistanceToSegmentXZ(bounds.center, start, end) - horizontalExtent <= halfWidth)
                RemoveCollider(collider);
        }
    }

    public static void CreateBannerBar(Transform parent, string name, Vector3 position, Quaternion rotation, float width)
    {
        GameObject root = CreateGroup(name, parent);
        root.transform.position = position;
        root.transform.rotation = rotation;
        CreatePrimitive(root.transform, "Beam_NoCollider", PrimitiveType.Cube, Vector3.zero, Quaternion.identity, new Vector3(width, 0.28f, 0.28f), L0OrcArenaMaterials.Wood, false);
        CreatePrimitive(root.transform, "RedFlag", PrimitiveType.Cube, new Vector3(0f, -0.85f, -0.05f), Quaternion.identity, new Vector3(width * 0.72f, 1.35f, 0.08f), L0OrcArenaMaterials.OrcRed, false);
    }

    public static void CreateSideFlag(Transform parent, string name, Vector3 position, Quaternion rotation, float scale)
    {
        GameObject root = CreateGroup(name, parent);
        root.transform.position = position;
        root.transform.rotation = rotation;
        CreatePrimitive(root.transform, "Pole_NoCollider", PrimitiveType.Cylinder, new Vector3(0f, 1.85f * scale, 0f), Quaternion.identity, new Vector3(0.07f * scale, 1.85f * scale, 0.07f * scale), L0OrcArenaMaterials.DarkWood, false);
        CreatePrimitive(root.transform, "Flag", PrimitiveType.Cube, new Vector3(0.55f * scale, 2.9f * scale, 0f), Quaternion.Euler(0f, 0f, -7f), new Vector3(1.05f * scale, 0.78f * scale, 0.06f * scale), L0OrcArenaMaterials.OrcRed, false);
    }

    public static void CreateDebugMarker(Transform parent, string name, Vector3 position, Color color, float scale = 1f)
    {
        GameObject root = CreateGroup("DEBUG_" + name, parent);
        root.transform.position = position + Vector3.up * (1.0f * scale);
        CreatePrimitive(root.transform, "Marker", PrimitiveType.Cylinder, Vector3.zero, Quaternion.identity, new Vector3(0.42f * scale, 1.0f * scale, 0.42f * scale), color, false, true);
        L0Props.CreateRouteSign(name, position + Vector3.up * 0.1f + Vector3.right * (1.1f * scale), Quaternion.identity, root.transform);
    }
}
