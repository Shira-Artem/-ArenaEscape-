using UnityEngine;

public class Door : MonoBehaviour
{
    public float openAngle = 90f;
    public float speed     = 3f;

    Quaternion _closed, _open;
    bool _isOpen;

    void Start()
    {
        _closed = transform.rotation;
        _open   = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation,
            _isOpen ? _open : _closed, speed * Time.deltaTime);
    }

    public void Toggle() => _isOpen = !_isOpen;
}
