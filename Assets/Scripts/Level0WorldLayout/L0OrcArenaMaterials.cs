using UnityEngine;

/// <summary>
/// Color palette and material creation for the modular orc arena.
/// </summary>
public static class L0OrcArenaMaterials
{
    public static readonly Color Road = new Color(0.64f, 0.50f, 0.30f);
    public static readonly Color GateGround = new Color(0.27f, 0.17f, 0.08f);
    public static readonly Color ArenaDirt = new Color(0.105f, 0.066f, 0.040f);
    public static readonly Color ArenaInner = new Color(0.070f, 0.046f, 0.031f);
    public static readonly Color PortalStone = new Color(0.14f, 0.13f, 0.13f);
    public static readonly Color DarkStone = new Color(0.10f, 0.095f, 0.10f);
    public static readonly Color DarkWood = new Color(0.11f, 0.065f, 0.040f);
    public static readonly Color Wood = new Color(0.32f, 0.19f, 0.09f);
    public static readonly Color HutRoof = new Color(0.09f, 0.055f, 0.040f);
    public static readonly Color OrcRed = new Color(0.48f, 0.025f, 0.020f);
    public static readonly Color Bone = new Color(0.72f, 0.62f, 0.48f);
    public static readonly Color Mountain = new Color(0.120f, 0.150f, 0.175f);
    public static readonly Color MountainDark = new Color(0.070f, 0.095f, 0.120f);
    public static readonly Color MountainFar = new Color(0.140f, 0.180f, 0.205f);
    public static readonly Color CliffNear = new Color(0.105f, 0.118f, 0.125f);
    public static readonly Color CliffRidge = new Color(0.055f, 0.068f, 0.078f);
    public static readonly Color AshMist = new Color(0.48f, 0.54f, 0.58f, 0.22f);
    public static readonly Color Fire = new Color(1.0f, 0.34f, 0.05f);

    public static Material Create(Color color, bool emissive = false, bool transparent = false)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        Material material = new Material(shader);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);

        if (emissive)
        {
            material.EnableKeyword("_EMISSION");
            if (material.HasProperty("_EmissionColor"))
                material.SetColor("_EmissionColor", color * 1.65f);
        }

        if (transparent)
            ConfigureTransparent(material, color);

        return material;
    }

    private static void ConfigureTransparent(Material material, Color color)
    {
        color.a = Mathf.Clamp01(color.a);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);

        if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
        if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
        if (material.HasProperty("_Mode")) material.SetFloat("_Mode", 3f);

        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }
}
