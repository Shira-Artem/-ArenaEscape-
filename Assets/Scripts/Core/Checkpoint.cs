using System.Collections;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    static Vector3 _pos;
    static int     _hp, _score;
    static bool    _has;

    public Renderer flag;
    bool    _hit;
    float   _pulseTimer;
    Color   _activeColor = new Color(0.1f, 1f, 0.35f);

    void OnTriggerEnter(Collider c)
    {
        if (_hit || !c.CompareTag("Player")) return;
        _hit = true;

        var ph = c.GetComponent<PlayerHealth>();
        _pos   = c.transform.position;
        _hp    = ph ? ph.currentHealth : 100;
        _score = GameManager.Instance ? GameManager.Instance.Score : 0;
        _has   = true;

        SaveSystem.Save(_pos, _hp, _score);

        // Красивое сообщение
        GameManager.Instance?.ShowMessage("✓ ЧЕКПОИНТ СОХРАНЁН", GameManager.MsgType.Checkpoint, 2.2f);

        // Звук и эффекты через FeedbackManager
        if (FeedbackManager.Instance != null)
        {
            FeedbackManager.Instance.PlayCheckpoint();

            // Золотой + зелёный взрыв
            FeedbackManager.Instance.SpawnBurst(transform.position + Vector3.up,
                new Color(1f, 0.9f, 0.1f), new Color(0.2f, 1f, 0.35f), 22, 4f, 0.12f);
            FeedbackManager.Instance.SpawnRing(transform.position + Vector3.up * 0.5f,
                new Color(0.2f, 1f, 0.35f), 28, 1.2f);
            FeedbackManager.Instance.SpawnRing(transform.position + Vector3.up * 1.2f,
                new Color(1f, 0.88f, 0.1f), 20, 0.7f);

            FeedbackManager.Instance.FloatText(transform.position + Vector3.up * 2f,
                "СОХРАНЕНО!", new Color(1f, 0.88f, 0.1f), 2f);
        }

        // Меняем цвет флага
        if (flag)
        {
            flag.material.color = _activeColor;
        }

        StartCoroutine(PulseEffect());
    }

    IEnumerator PulseEffect()
    {
        if (flag == null) yield break;

        // 3 секунды пульсируем после активации
        float timer = 0f;
        while (timer < 3f)
        {
            timer += Time.deltaTime;
            float pulse = 0.5f + 0.5f * Mathf.Sin(timer * 8f);
            Color c = Color.Lerp(_activeColor, new Color(0.8f, 1f, 0.2f), pulse * 0.5f);
            flag.material.color = c;
            yield return null;
        }

        // Установить финальный цвет
        flag.material.color = _activeColor;
        // Добавить эмиссию для свечения
        flag.material.EnableKeyword("_EMISSION");
        flag.material.SetColor("_EmissionColor", _activeColor * 0.5f);
    }

    public static void Respawn(GameObject player)
    {
        if (!_has) return;
        player.transform.position = _pos;
        var ph = player.GetComponent<PlayerHealth>();
        if (ph) { ph.currentHealth = _hp; ph.Respawn(); }
        GameManager.Instance?.SetScore(_score);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}
