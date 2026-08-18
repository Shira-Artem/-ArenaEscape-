using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public string    prompt     = "Нажмите [F]";
    public bool      oneTime    = true;
    public UnityEvent onInteract;

    bool _inRange, _used;

    void OnTriggerEnter(Collider c) { if (c.CompareTag("Player")) _inRange = true;  }
    void OnTriggerExit (Collider c) { if (c.CompareTag("Player")) _inRange = false; }

    void Update()
    {
        if (_inRange && !(oneTime && _used) && Input.GetKeyDown(KeyCode.F))
        { onInteract?.Invoke(); if (oneTime) _used = true; }
    }

    void OnGUI()
    {
        if (!_inRange || (oneTime && _used)) return;
        GUIStyle s = new GUIStyle(GUI.skin.box) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
        s.normal.textColor = Color.white;
        GUI.Box(new Rect(Screen.width/2f-110, Screen.height-130, 220, 36), prompt, s);
    }
}
