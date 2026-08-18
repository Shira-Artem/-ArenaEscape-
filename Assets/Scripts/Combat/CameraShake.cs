using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Vector3 _origin;
    bool    _shaking;

    void Awake() => _origin = transform.localPosition;

    public void Shake(float magnitude = 0.15f, float duration = 0.25f)
    {
        if (_shaking) StopAllCoroutines();
        StartCoroutine(Do(magnitude, duration));
    }

    public void ShakeHard() => Shake(0.3f, 0.35f);
    public void ShakeKill() => Shake(0.2f, 0.18f);

    IEnumerator Do(float mag, float dur)
    {
        _shaking = true;
        float t = 0;
        while (t < dur)
        {
            t += Time.deltaTime;
            float d = 1f - t / dur;
            transform.localPosition = _origin + new Vector3(
                Random.Range(-1f, 1f) * mag * d,
                Random.Range(-1f, 1f) * mag * d, 0);
            yield return null;
        }
        transform.localPosition = _origin;
        _shaking = false;
    }
}
