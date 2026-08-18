using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    float _alpha;
    Texture2D _black;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _black = new Texture2D(1, 1);
        _black.SetPixel(0, 0, Color.black);
        _black.Apply();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void LoadScene(string sceneName)
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.FadeAndLoad(sceneName));
        else
            SceneManager.LoadScene(sceneName);
    }

    public static void FadeIn()
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.DoFadeIn());
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        float t = 0f;
        while (t < 0.4f)
        {
            t += Time.unscaledDeltaTime;
            _alpha = Mathf.Clamp01(t / 0.4f);
            yield return null;
        }
        _alpha = 1f;

        SceneManager.LoadScene(sceneName);
        yield return null;

        StartCoroutine(DoFadeIn());
    }

    IEnumerator DoFadeIn()
    {
        _alpha = 1f;
        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.unscaledDeltaTime;
            _alpha = 1f - Mathf.Clamp01(t / 0.5f);
            yield return null;
        }
        _alpha = 0f;
    }

    void OnGUI()
    {
        if (_alpha <= 0f) return;
        GUI.depth = -1000;
        GUI.color = new Color(0f, 0f, 0f, _alpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _black);
        GUI.color = Color.white;
    }
}
