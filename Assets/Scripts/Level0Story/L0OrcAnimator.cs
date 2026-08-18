using UnityEngine;

/// <summary>
/// Псевдо-анимации орков через Lerp scale/position (без костей):
///   — Берсерк: быстрое покачивание головы
///   — Шаман: парящий магический шар
///   — Босс: пульсация красного свечения при низком HP
///   — Танк: медленное покачивание щита
///
/// Добавляется L0OrcFactory при создании орка.
/// </summary>
public class L0OrcAnimator : MonoBehaviour
{
    public L0OrcFactory.OrcType orcType;
    public Transform headTransform;
    public Transform orbTransform;
    public Transform shieldTransform;
    public Transform colossusBodyTransform;
    public Transform spearTransform;

    Vector3 _headBaseScale;
    Vector3 _orbBasePos;
    Vector3 _shieldBasePos;
    Vector3 _colossusBodyBasePos;
    Vector3 _spearBasePos;

    Light _bossGlow;
    Light _colossusGlow;
    EnemyAI _ai;
    float _phase;
    bool _init;

    void Start()
    {
        _phase = Random.value * Mathf.PI * 2f;
        _ai = GetComponent<EnemyAI>();

        if (headTransform != null) _headBaseScale = headTransform.localScale;
        if (orbTransform != null) _orbBasePos = orbTransform.localPosition;
        if (shieldTransform != null) _shieldBasePos = shieldTransform.localPosition;
        if (spearTransform != null) _spearBasePos = spearTransform.localPosition;

        if (orcType == L0OrcFactory.OrcType.Boss)
        {
            var glowGo = new GameObject("BossGlow");
            glowGo.transform.SetParent(transform, false);
            glowGo.transform.localPosition = new Vector3(0f, 1.2f, 0.2f);
            _bossGlow = glowGo.AddComponent<Light>();
            _bossGlow.type = LightType.Point;
            _bossGlow.color = new Color(1f, 0.15f, 0.05f);
            _bossGlow.range = 4f;
            _bossGlow.intensity = 0f;
        }

        if (orcType == L0OrcFactory.OrcType.Colossus)
        {
            // Оранжевое свечение у ног — как жар земли под ним
            var glowGo = new GameObject("ColossusGlow");
            glowGo.transform.SetParent(transform, false);
            glowGo.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            _colossusGlow = glowGo.AddComponent<Light>();
            _colossusGlow.type = LightType.Point;
            _colossusGlow.color = new Color(1f, 0.45f, 0.05f);
            _colossusGlow.range = 9f;
            _colossusGlow.intensity = 1.2f;

            if (colossusBodyTransform != null)
                _colossusBodyBasePos = colossusBodyTransform.localPosition;
        }

        // Wolf / Goblin используют colossusBodyTransform для анимации тела
        if ((orcType == L0OrcFactory.OrcType.Wolf || orcType == L0OrcFactory.OrcType.Goblin)
            && colossusBodyTransform != null)
        {
            _colossusBodyBasePos = colossusBodyTransform.localPosition;
        }

        // Necromancer — orb парит как у Shaman
        if (orcType == L0OrcFactory.OrcType.Necromancer && orbTransform != null)
            _orbBasePos = orbTransform.localPosition;

        _init = true;
    }

    void Update()
    {
        if (!_init) return;
        if (Time.frameCount % 2 != 0) return;
        float t = Time.time + _phase;

        switch (orcType)
        {
            case L0OrcFactory.OrcType.Berserker:
                AnimateBerserker(t);
                break;
            case L0OrcFactory.OrcType.Shaman:
                AnimateShaman(t);
                break;
            case L0OrcFactory.OrcType.Boss:
                AnimateBoss(t);
                break;
            case L0OrcFactory.OrcType.Tank:
                AnimateTank(t);
                break;
            case L0OrcFactory.OrcType.Colossus:
                AnimateColossus(t);
                break;
            case L0OrcFactory.OrcType.Wolf:
                AnimateWolf(t);
                break;
            case L0OrcFactory.OrcType.Goblin:
                AnimateGoblin(t);
                break;
            case L0OrcFactory.OrcType.Spearman:
                AnimateSpearman(t);
                break;
            case L0OrcFactory.OrcType.Necromancer:
                AnimateNecromancer(t);
                break;
            case L0OrcFactory.OrcType.Warchief:
                AnimateWarchief(t);
                break;
            case L0OrcFactory.OrcType.PoisonArcher:
                AnimatePoisonArcher(t);
                break;
        }
    }

    void AnimateBerserker(float t)
    {
        if (headTransform == null) return;
        float bob = Mathf.Sin(t * 8f) * 0.06f;
        headTransform.localScale = _headBaseScale + new Vector3(bob, -bob * 0.5f, bob * 0.3f);
    }

    void AnimateShaman(float t)
    {
        if (orbTransform == null) return;
        float hover = Mathf.Sin(t * 2.5f) * 0.12f;
        float sway = Mathf.Cos(t * 1.8f) * 0.05f;
        orbTransform.localPosition = _orbBasePos + new Vector3(sway, hover, 0f);

        float pulse = 0.85f + Mathf.Sin(t * 3f) * 0.15f;
        orbTransform.localScale = Vector3.one * 0.26f * pulse;
    }

    void AnimateBoss(float t)
    {
        if (_bossGlow == null || _ai == null) return;
        float hpRatio = (float)_ai.CurrentHp / _ai.maxHp;
        if (hpRatio < 0.4f)
        {
            float pulse = Mathf.Abs(Mathf.Sin(t * 4f));
            _bossGlow.intensity = Mathf.Lerp(1.5f, 3.5f, pulse);
            _bossGlow.range = Mathf.Lerp(4f, 7f, pulse);
        }
        else
        {
            _bossGlow.intensity = 0.3f;
        }
    }

    void AnimateTank(float t)
    {
        if (shieldTransform == null) return;
        float sway = Mathf.Sin(t * 1.2f) * 0.03f;
        shieldTransform.localPosition = _shieldBasePos + new Vector3(sway, 0f, 0f);
    }

    void AnimateWolf(float t)
    {
        if (colossusBodyTransform == null) return;
        // Быстрое покачивание тела — имитация рыси/галопа
        float sway = Mathf.Sin(t * 12f) * 0.04f;
        colossusBodyTransform.localPosition = _colossusBodyBasePos + new Vector3(sway, Mathf.Abs(Mathf.Sin(t * 12f)) * 0.025f, 0f);
    }

    void AnimateGoblin(float t)
    {
        if (colossusBodyTransform == null) return;
        // Быстрое подпрыгивание — нервный гоблин
        float bounce = Mathf.Abs(Mathf.Sin(t * 8f)) * 0.04f;
        colossusBodyTransform.localPosition = _colossusBodyBasePos + new Vector3(0f, bounce, 0f);
    }

    void AnimateColossus(float t)
    {
        float hpRatio = (_ai != null) ? (float)_ai.CurrentHp / _ai.maxHp : 1f;
        bool enraged  = hpRatio < 0.5f;

        if (colossusBodyTransform != null)
        {
            float speed = enraged ? 1.6f : 0.6f;
            float amp   = enraged ? 0.045f : 0.022f;
            float sway  = Mathf.Sin(t * speed) * amp;
            colossusBodyTransform.localPosition = _colossusBodyBasePos + new Vector3(sway, 0f, 0f);
        }

        if (_colossusGlow != null)
        {
            float pSpeed = enraged ? 4.0f : 1.0f;
            float pulse  = 0.5f + Mathf.Abs(Mathf.Sin(t * pSpeed)) * 0.5f;
            _colossusGlow.intensity = enraged
                ? Mathf.Lerp(2.5f, 5.5f, pulse)
                : Mathf.Lerp(0.8f, 2.0f, pulse);
            _colossusGlow.range = enraged
                ? Mathf.Lerp(10f, 18f, pulse)
                : 9f;
            _colossusGlow.color = enraged
                ? Color.Lerp(new Color(1f, 0.45f, 0.05f), new Color(1f, 0.20f, 0.02f), pulse)
                : new Color(1f, 0.45f, 0.05f);
        }
    }

    // Копьеносец: медленное покачивание копья вперёд-назад
    void AnimateSpearman(float t)
    {
        if (spearTransform == null) return;
        float bob = Mathf.Sin(t * 1.5f) * 0.04f;
        spearTransform.localPosition = _spearBasePos + new Vector3(0f, bob, bob * 0.5f);
    }

    // Некромант: парящий зелёный череп + покачивание рук вверх-вниз
    void AnimateNecromancer(float t)
    {
        if (orbTransform == null) return;
        float hover = Mathf.Sin(t * 2.0f) * 0.10f;
        float sway  = Mathf.Cos(t * 1.4f) * 0.04f;
        orbTransform.localPosition = _orbBasePos + new Vector3(sway, hover, 0f);

        float pulse = 0.80f + Mathf.Sin(t * 4f) * 0.20f;
        orbTransform.localScale = Vector3.one * 0.22f * pulse;
    }

    // Вождь: покачивание топора над головой
    void AnimateWarchief(float t)
    {
        if (headTransform == null) return;
        float hpRatio = (_ai != null) ? (float)_ai.CurrentHp / _ai.maxHp : 1f;
        bool enraged  = hpRatio < 0.5f;
        float speed   = enraged ? 4f : 2f;
        float amp     = enraged ? 0.07f : 0.04f;
        float bob     = Mathf.Sin(t * speed) * amp;
        // лёгкий наклон головы при ярости
        headTransform.localScale = _headBaseScale + new Vector3(bob * 0.3f, -bob * 0.2f, 0f);
    }

    // Отравитель: тик-так головой при атаке
    void AnimatePoisonArcher(float t)
    {
        if (headTransform == null) return;
        float twitch = Mathf.Sin(t * 6f) * 0.025f;
        headTransform.localScale = _headBaseScale + new Vector3(twitch, 0f, 0f);
    }
}
