using UnityEngine;
using UnityEngine.AI;

public class L0RaidOrcRunner : MonoBehaviour
{
    public Vector3 gateTarget;
    public float moveSpeed = 3.0f;
    public float stopDistance = 6.0f;
    public bool disableWhenEnemyAiCanMove = false;

    [Header("Ally Attack")]
    public float allyAttackRange = 2.6f;
    public float allyAttackCooldown = 1.9f;

    private NavMeshAgent agent;
    private EnemyAI _ai;
    private Vector3 lastPosition;
    private float nextMovementCheckTime;
    private bool agentWasMoving;
    private float _nextAllyAtk;
    private L0GateAllySoldier[] _soldiersCache;
    private float _soldierCacheTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        _ai = GetComponent<EnemyAI>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (ReachedGateTarget())
        {
            TryAttackAlly();
            return;
        }

        if (AgentCanHandleMovement())
            return;

        MoveManuallyToGate();
    }

    private void TryAttackAlly()
    {
        if (_ai == null || _ai.state == EnemyAI.State.Dead) return;
        if (Time.time < _nextAllyAtk) return;

        // Кэш стражников обновляем раз в 1.5с — не каждый кадр.
        if (_soldiersCache == null || Time.time > _soldierCacheTime)
        {
            _soldiersCache = Object.FindObjectsByType<L0GateAllySoldier>(FindObjectsSortMode.None);
            _soldierCacheTime = Time.time + 1.5f;
        }

        float bestSqr = allyAttackRange * allyAttackRange;
        L0GateAllySoldier nearest = null;
        for (int i = 0; i < _soldiersCache.Length; i++)
        {
            var s = _soldiersCache[i];
            if (s == null || s.IsDead) continue;
            float d = (s.transform.position - transform.position).sqrMagnitude;
            if (d < bestSqr) { bestSqr = d; nearest = s; }
        }

        if (nearest == null) return;

        _nextAllyAtk = Time.time + allyAttackCooldown;
        int dmg = _ai != null ? _ai.damage : 15;
        nearest.ApplyDamage(dmg);
        FeedbackManager.Instance?.FloatText(
            nearest.transform.position + Vector3.up * 1.8f,
            "УДАР!", new Color(1f, 0.3f, 0.08f), 0.5f);
    }

    private bool AgentCanHandleMovement()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return false;

        if (disableWhenEnemyAiCanMove)
            return true;

        if (Time.time >= nextMovementCheckTime)
        {
            Vector3 delta = transform.position - lastPosition;
            delta.y = 0f;
            agentWasMoving = delta.sqrMagnitude > 0.0025f || agent.velocity.sqrMagnitude > 0.01f;
            lastPosition = transform.position;
            nextMovementCheckTime = Time.time + 0.25f;
        }

        return agentWasMoving;
    }

    private bool ReachedGateTarget()
    {
        Vector3 toTarget = gateTarget - transform.position;
        toTarget.y = 0f;
        return toTarget.sqrMagnitude <= stopDistance * stopDistance;
    }

    private void MoveManuallyToGate()
    {
        Vector3 current = transform.position;
        Vector3 target = new Vector3(gateTarget.x, current.y, gateTarget.z);
        Vector3 toTarget = target - current;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude <= 0.0001f)
            return;

        Vector3 direction = toTarget.normalized;
        transform.position = current + direction * (moveSpeed * Time.deltaTime);

        Quaternion look = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 8f);
    }
}
