using System.Collections;
using UnityEngine;

public class KingAbility : MonoBehaviour
{
    public static KingAbility Instance { get; private set; }

    public bool unlocked;
    public float charge;       // 0..1
    public float chargePerKill = 0.20f;
    public int fireballDamage = 80;
    public float fireballSpeed = 40f;
    public float fireballRadius = 5f;
    public float fireballMaxDist = 80f;

    float _cooldown;
    Camera _cam;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        // Camera.main меняется при смене сцены — сбрасываем кэш
        _cam = null;
    }

    void Update()
    {
        if (!unlocked) return;
        if (_cooldown > 0f) _cooldown -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Z) && charge >= 1f && _cooldown <= 0f)
            Fire();
    }

    public void AddCharge()
    {
        if (!unlocked) return;
        charge = Mathf.Clamp01(charge + chargePerKill);
        if (charge >= 1f)
            GameManager.Instance?.ShowMessage("Заряд [Q] готов!", GameManager.MsgType.Default, 1.5f);
    }

    void Fire()
    {
        charge = 0f;
        _cooldown = 0.5f;

        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        Vector3 origin = _cam.transform.position + _cam.transform.forward * 2f;
        Vector3 dir = _cam.transform.forward;

        Fireball.Launch(origin, dir, fireballDamage, fireballSpeed, fireballRadius, fireballMaxDist);

        var shake = _cam.GetComponent<CameraShake>();
        shake?.ShakeHard();

        var fb = FeedbackManager.Instance;
        if (fb != null)
        {
            fb.PlayCrit();
            fb.SpawnBurst(origin, new Color(1f, 0.6f, 0.08f), new Color(1f, 0.25f, 0f), 40, 6f, 0.25f);
            fb.SpawnBurst(origin, new Color(1f, 0.9f, 0.3f), new Color(1f, 0.5f, 0f), 20, 3f, 0.12f);
            fb.SpawnRing(origin, new Color(1f, 0.45f, 0.05f), 30, 2.5f);
        }

        HitStop.Freeze(0.04f);
    }

    void OnGUI()
    {
        if (!unlocked) return;

        int sw = Screen.width;
        float barW = 160f, barH = 14f;
        float x = 18f, y = 112f;

        GUI.color = new Color(0.12f, 0.06f, 0.02f, 0.75f);
        GUI.DrawTexture(new Rect(x - 1, y - 1, barW + 2, barH + 2), Texture2D.whiteTexture);

        Color fill = charge >= 1f
            ? Color.Lerp(new Color(1f, 0.5f, 0f), new Color(1f, 0.8f, 0.1f), Mathf.PingPong(Time.unscaledTime * 3f, 1f))
            : new Color(0.9f, 0.35f, 0.05f);
        GUI.color = fill;
        GUI.DrawTexture(new Rect(x, y, barW * charge, barH), Texture2D.whiteTexture);
        GUI.color = Color.white;

        var style = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
        style.normal.textColor = charge >= 1f ? new Color(1f, 0.9f, 0.2f) : new Color(0.85f, 0.6f, 0.3f);
        string label = charge >= 1f ? "Z ОГНЕННЫЙ ШАР!" : $"Z Заряд: {Mathf.RoundToInt(charge * 100)}%";
        GUI.Label(new Rect(x, y + barH + 1, barW, 20), label, style);
    }
}
