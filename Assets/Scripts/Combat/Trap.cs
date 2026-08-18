using System.Collections;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public int damage = 25;
    public float tickDelay = 0.55f;
    public bool alwaysActive = true;

    Renderer[] _renderers;
    Light[] _lights;
    bool _active = true;
    float _lastDamageTime = -999f;

    void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _lights = GetComponentsInChildren<Light>(true);

        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void Start()
    {
        // Дочерние визуальные части создаются после AddComponent, поэтому обновляем список тут.
        _renderers = GetComponentsInChildren<Renderer>(true);
        _lights = GetComponentsInChildren<Light>(true);
        _active = true;
        StartCoroutine(ReadablePulse());
    }

    void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryDamage(other);
    }

    void TryDamage(Collider other)
    {
        if (!_active || other == null || !other.CompareTag("Player")) return;
        if (Time.unscaledTime < _lastDamageTime + tickDelay) return;

        var hp = other.GetComponent<PlayerHealth>();
        if (hp == null) return;

        _lastDamageTime = Time.unscaledTime;
        hp.TakeDamage(damage, transform.position);
        GameManager.Instance?.ShowMessage($"-{damage} HP: ловушка!", GameManager.MsgType.Damage, 0.8f);
    }

    IEnumerator ReadablePulse()
    {
        while (true)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 2.4f;
                float p = 0.5f + Mathf.Sin(Time.time * 8f) * 0.5f;
                Color hot = Color.Lerp(new Color(0.85f, 0.05f, 0.02f), new Color(1f, 0.78f, 0.05f), p);

                if (_renderers != null)
                {
                    foreach (var r in _renderers)
                    {
                        if (r == null || r.material == null) continue;
                        string n = r.gameObject.name.ToLowerInvariant();
                        if (n.Contains("hot") || n.Contains("lamp") || n.Contains("baseplate"))
                        {
                            r.material.color = hot;
                            if (r.material.HasProperty("_EmissionColor"))
                            {
                                r.material.EnableKeyword("_EMISSION");
                                r.material.SetColor("_EmissionColor", hot * 0.7f);
                            }
                        }
                    }
                }

                if (_lights != null)
                {
                    foreach (var l in _lights)
                    {
                        if (l == null) continue;
                        l.intensity = 1.0f + p * 1.2f;
                    }
                }

                yield return null;
            }

            // Ловушка всегда опасна, поэтому игрок больше не будет думать, что она "не работает".
            if (!alwaysActive) _active = !_active;
            else _active = true;
        }
    }
}
