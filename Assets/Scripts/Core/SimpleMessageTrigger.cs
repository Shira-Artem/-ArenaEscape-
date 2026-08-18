using UnityEngine;

public class SimpleMessageTrigger : MonoBehaviour
{
    public string message = "";
    bool _done;

    void OnTriggerEnter(Collider other)
    {
        if (_done || !other.CompareTag("Player")) return;
        _done = true;
        GameManager.Instance?.ShowMessage(message, 1.5f);
        Destroy(gameObject, 0.5f);
    }
}
