using UnityEngine;

/// <summary>
/// Автономная подсказка для переходов.
/// Без Canvas, без TextMeshPro. Создаётся сама при первом вызове Show().
/// </summary>
public class TransitionHintUI : MonoBehaviour
{
    private static TransitionHintUI instance;

    private string currentText = "";
    private float clearTime;
    private GUIStyle boxStyle;

    public static void Show(string text, float duration)
    {
        if (instance == null)
        {
            GameObject uiObj = new GameObject("[TransitionHintUI_Runtime]");
            instance = uiObj.AddComponent<TransitionHintUI>();
            DontDestroyOnLoad(uiObj);
        }

        instance.currentText = text;
        instance.clearTime = Time.time + duration;
    }

    private void OnGUI()
    {
        if (Time.time > clearTime || string.IsNullOrEmpty(currentText))
            return;

        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.alignment = TextAnchor.MiddleCenter;
            boxStyle.fontSize = 18;
            boxStyle.normal.textColor = Color.white;
            boxStyle.wordWrap = true;
        }

        float width = Mathf.Min(520f, Screen.width - 80f);
        float height = 74f;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height * 0.74f;

        Color oldBg = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0f, 0f, 0f, 0.88f);
        GUI.Box(new Rect(x, y, width, height), currentText, boxStyle);
        GUI.backgroundColor = oldBg;
    }
}
