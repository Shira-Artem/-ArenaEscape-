using UnityEngine;

/// <summary>
/// Вешается на каждого врага автоматически из SceneBuilder.
/// Если игрок прыгает сверху — враг умирает.
/// Если касается сбоку/снизу — игрок получает урон.
/// </summary>
public class EnemyHitDetector : MonoBehaviour
{
    public int contactDamage = 15;   // урон при боковом касании

    EnemyAI      _ai;
    PlayerHealth _ph;
    float        _lastDmg;

    void Start() => _ai = GetComponent<EnemyAI>();

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Эту функцию вызывает CharacterController игрока
        // но нам нужен OnCollisionEnter на враге — см. ниже
    }

    void OnCollisionEnter(Collision col)
    {
        if (_ai == null || _ai.state == EnemyAI.State.Dead) return;
        if (!col.gameObject.CompareTag("Player")) return;

        // Определяем: игрок сверху или сбоку?
        Vector3 dir = col.contacts[0].normal;

        if (dir.y < -0.5f)
        {
            // Нормаль смотрит вниз → игрок ударил сверху
            _ai.SetNextHitBonus(RunScoreManager.KillBonus.Stomp);
            _ai.TakeDamage(999);

            // Подбрасываем игрока чуть вверх (отскок)
            var cc = col.gameObject.GetComponent<CharacterController>();
            // CharacterController не имеет Rigidbody — используем PlayerController
            var pc = col.gameObject.GetComponent<PlayerController>();
            if (pc != null) pc.BounceUp(5f);

            GameManager.Instance?.ShowMessage("Стомп! +25", 1f);
        }
        else
        {
            // Боковое касание — урон игроку
            if (Time.time < _lastDmg + 1f) return;
            _lastDmg = Time.time;

            var ph = col.gameObject.GetComponent<PlayerHealth>();
            ph?.TakeDamage(contactDamage, transform.position);
        }
    }
}
