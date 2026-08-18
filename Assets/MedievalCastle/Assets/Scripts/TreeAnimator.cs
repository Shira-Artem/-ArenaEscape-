using UnityEngine;

/// <summary>
/// Лёгкая анимация "дыхания" для деревьев и кустов.
/// Компонент автоматически добавляется на родительский GameObject дерева из CastleNatureAssets.
/// </summary>
public class TreeAnimator : MonoBehaviour
{
    [Header("Breathing Motion")]
    public float amplitude = 0.004f;
    public float speed = 0.35f;
    public float tiltAmount = 1.4f;

    private Vector3 basePos;
    private Quaternion baseRot;
    private float phase;

    private void Start()
    {
        basePos = transform.position;
        baseRot = transform.rotation;

        // Стабильная фаза от позиции объекта. Так деревья качаются не одинаково.
        phase = Mathf.Abs(
            transform.position.x * 0.73f +
            transform.position.z * 1.37f +
            GetInstanceID() * 0.01f
        );
    }

    private void Update()
    {
        float wave = Mathf.Sin(Time.time * speed + phase);

        transform.position = basePos + Vector3.up * (wave * amplitude);
        transform.rotation = baseRot * Quaternion.Euler(
            wave * tiltAmount * 0.25f,
            0f,
            Mathf.Sin(Time.time * speed * 0.7f + phase) * tiltAmount
        );
    }
}
