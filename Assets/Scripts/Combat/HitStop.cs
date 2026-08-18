using System.Collections;
using UnityEngine;

/// HitStop — микро-стоп времени для «веса» попаданий.
/// Переиспользуемый persistent-раннер: можно звать из снарядов, которые
/// уничтожаются в момент попадания (их собственная корутина бы оборвалась).
public class HitStop : MonoBehaviour
{
    static HitStop _inst;
    static float   _resumeAt = -1f;   // realtime-момент, когда вернуть timeScale=1
    static bool    _frozen;

    static HitStop Instance
    {
        get
        {
            if (_inst == null)
            {
                var go = new GameObject("~HitStop");
                DontDestroyOnLoad(go);
                _inst = go.AddComponent<HitStop>();
            }
            return _inst;
        }
    }

    public static void CancelAll()
    {
        if (_inst != null) _inst.StopAllCoroutines();
        _frozen = false;
        _resumeAt = -1f;
    }

    public static void Freeze(float duration)
    {
        if (duration <= 0f) return;
        if (Time.timeScale == 0f && !_frozen) return; // уже стоим по другой причине (меню/пауза)

        var i = Instance;
        float target = Time.realtimeSinceStartup + duration;
        if (target > _resumeAt) _resumeAt = target;

        if (!_frozen)
        {
            _frozen = true;
            Time.timeScale = 0f;
            i.StartCoroutine(i.Run());
        }
    }

    IEnumerator Run()
    {
        while (Time.realtimeSinceStartup < _resumeAt)
            yield return null;
        Time.timeScale = 1f;
        _frozen = false;
        _resumeAt = -1f;
    }
}
