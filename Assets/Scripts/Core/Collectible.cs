using System.Collections;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int   value     = 10;
    public bool  isHealth  = false;
    public int   healAmt   = 30;
    public float bobSpeed  = 2f;
    public float rotSpeed  = 80f;

    Vector3 _start;
    bool    _collected;

    void Start() => _start = transform.position;

    void Update()
    {
        if (_collected) return;
        transform.position = new Vector3(
            _start.x,
            _start.y + Mathf.Sin(Time.time * bobSpeed) * 0.2f,
            _start.z);
        transform.Rotate(Vector3.up, rotSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_collected || !other.CompareTag("Player")) return;
        _collected = true;

        if (isHealth)
        {
            other.GetComponent<PlayerHealth>()?.Heal(healAmt);
            GameManager.Instance?.ShowMessage($"+{healAmt} HP", GameManager.MsgType.Heal, 1.3f);

            if (FeedbackManager.Instance != null)
            {
                FeedbackManager.Instance.PlayHealth();
                FeedbackManager.Instance.SpawnBurst(transform.position,
                    new Color(0.2f, 1f, 0.3f), new Color(0.1f, 0.7f, 0.2f), 16, 3f);
                FeedbackManager.Instance.SpawnRing(transform.position,
                    new Color(0.2f, 0.9f, 0.3f), 18, 0.5f);
                FeedbackManager.Instance.FloatText(transform.position,
                    $"+{healAmt} HP", new Color(0.25f, 1f, 0.45f), 1.4f);
            }
        }
        else
        {
            GameManager.Instance?.AddScore(value);
            GameManager.Instance?.ShowMessage($"+{value} очков!", GameManager.MsgType.Coin, 0.9f);

            if (FeedbackManager.Instance != null)
            {
                FeedbackManager.Instance.PlayCoin();
                FeedbackManager.Instance.SpawnBurst(transform.position,
                    new Color(1f, 0.9f, 0.1f), new Color(1f, 0.55f, 0.0f), 14, 4f, 0.1f);
                FeedbackManager.Instance.FloatText(transform.position,
                    $"+{value}", new Color(1f, 0.9f, 0.1f), 1.2f);
            }
        }

        StartCoroutine(VanishFX());
    }

    IEnumerator VanishFX()
    {
        var rend = GetComponent<Renderer>();
        float t = 0f;
        Vector3 origScale = transform.localScale;

        while (t < 0.14f)
        {
            t += Time.deltaTime;
            float p = t / 0.14f;
            // Быстрый "поп" — чуть вырастаем, потом исчезаем
            float s = 1f + Mathf.Sin(p * Mathf.PI) * 0.55f;
            transform.localScale = origScale * s;

            if (rend)
            {
                Color c = rend.material.color;
                c.a = 1f - p;
                rend.material.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}
