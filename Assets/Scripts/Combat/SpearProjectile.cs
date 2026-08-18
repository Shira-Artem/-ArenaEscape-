using System.Collections;
using UnityEngine;

public class SpearProjectile : MonoBehaviour
{
    const float SPEED = 36f;
    const float GRAVITY = 6f;

    int _damage;
    Vector3 _vel;
    bool _hit;

    static Mesh _shaftMesh, _tipMesh;
    static Material _shaftMat, _tipMat;

    public static GameObject Fire(Vector3 pos, Vector3 dir, int damage, GameObject owner)
    {
        var go = new GameObject("Spear");
        go.transform.position = pos;
        go.layer = owner.layer;

        var proj = go.AddComponent<SpearProjectile>();
        proj._damage = damage;
        proj._vel = dir.normalized * SPEED;

        BuildVisual(go.transform);
        Destroy(go, 5f);
        return go;
    }

    static void BuildVisual(Transform root)
    {
        EnsureMeshes();

        var shaft = new GameObject("Shaft");
        shaft.transform.SetParent(root, false);
        shaft.AddComponent<MeshFilter>().sharedMesh = _shaftMesh;
        shaft.AddComponent<MeshRenderer>().sharedMaterial = _shaftMat;

        var tip = new GameObject("Tip");
        tip.transform.SetParent(root, false);
        tip.transform.localPosition = new Vector3(0, 0.6f, 0);
        tip.AddComponent<MeshFilter>().sharedMesh = _tipMesh;
        tip.AddComponent<MeshRenderer>().sharedMaterial = _tipMat;
    }

    static void EnsureMeshes()
    {
        if (_shaftMesh != null) return;

        _shaftMesh = new Mesh();
        _shaftMesh.vertices = new[] {
            new Vector3(-0.04f, -0.55f, -0.04f), new Vector3(0.04f, -0.55f, -0.04f),
            new Vector3(0.04f, -0.55f, 0.04f), new Vector3(-0.04f, -0.55f, 0.04f),
            new Vector3(-0.03f, 0.5f, -0.03f), new Vector3(0.03f, 0.5f, -0.03f),
            new Vector3(0.03f, 0.5f, 0.03f), new Vector3(-0.03f, 0.5f, 0.03f),
        };
        _shaftMesh.triangles = new[] {
            0,1,5, 0,5,4, 1,2,6, 1,6,5, 2,3,7, 2,7,6, 3,0,4, 3,4,7
        };
        _shaftMesh.RecalculateNormals();

        _tipMesh = new Mesh();
        _tipMesh.vertices = new[] {
            Vector3.zero,
            new Vector3(-0.07f,0,-0.07f), new Vector3(0.07f,0,-0.07f),
            new Vector3(0.07f,0,0.07f), new Vector3(-0.07f,0,0.07f),
            Vector3.up * 0.25f
        };
        _tipMesh.triangles = new[] {
            0,2,1, 0,3,2, 0,4,3, 0,1,4,
            5,1,2, 5,2,3, 5,3,4, 5,4,1
        };
        _tipMesh.RecalculateNormals();

        var lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
        _shaftMat = new Material(lit) { color = new Color(0.4f, 0.28f, 0.12f) };
        _tipMat   = new Material(lit) { color = new Color(1f, 0.85f, 0.1f) };
    }

    void Update()
    {
        if (_hit) return;

        float dt = Time.deltaTime;
        _vel.y -= GRAVITY * dt;

        Vector3 move = _vel * dt;
        float dist = move.magnitude;

        if (dist > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(_vel.normalized);

            if (Physics.Raycast(transform.position, _vel.normalized, out RaycastHit hit, dist + 0.3f))
            {
                var enemy = hit.collider.GetComponentInParent<EnemyAI>();
                if (enemy != null && enemy.state != EnemyAI.State.Dead)
                {
                    enemy.TakeDamage(_damage);
                    GameAudioManager.Instance?.Play(GameAudioManager.SFX.Hit, 0.8f);
                    GameManager.Instance?.ShowMessage($"↟ -{_damage}!", 0.55f);
                    Camera.main?.GetComponent<CameraShake>()?.Shake(0.28f, 0.22f);
                    DoHitStop(0.065f);
                    StickAndDie(hit.point);
                    return;
                }

                if (!hit.collider.isTrigger)
                {
                    StickAndDie(hit.point);
                    return;
                }
            }
        }

        transform.position += move;
    }

    void StickAndDie(Vector3 pos)
    {
        _hit = true;
        transform.position = pos;
        Destroy(gameObject, 2f);
    }

    void DoHitStop(float dur)
    {
        HitStop.Freeze(dur);
    }
}
