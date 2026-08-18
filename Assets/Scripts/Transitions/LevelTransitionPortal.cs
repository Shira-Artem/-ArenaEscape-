using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

/// <summary>
/// Портал перехода между зонами.
/// Не зависит от квестов, орков или GameProgressManager.
/// Внешняя логика может вызвать Unlock().
/// </summary>
public class LevelTransitionPortal : MonoBehaviour
{
    [Header("Scene")]
    public string nextSceneName = "one";
    public bool loadSceneOnUse = false;

    [Header("Lock")]
    public bool requireUnlock = true;
    public bool isUnlocked = false;
    public string lockedMessage = "Проход закрыт. Сначала выполни цель зоны.";
    public string unlockedMessage = "Путь открыт.";
    public KeyCode interactKey = KeyCode.F;

    [Header("Interaction")]
    public float interactionRadius = 3.5f;
    public UnityEvent onTransitionUsed;

    [Header("Visual")]
    public bool createVisualMarker = true;

    private Transform playerTransform;
    private bool visualBuilt;

    private void Start()
    {
        SetupTrigger();

        if (createVisualMarker && !visualBuilt)
            BuildVisualMarker();
    }

    private void Update()
    {
        FindPlayerIfNeeded();

        if (playerTransform == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance > interactionRadius)
            return;

        if (!CanUse())
        {
            TransitionHintUI.Show(lockedMessage, 0.08f);
            return;
        }

        TransitionHintUI.Show("Нажми [" + interactKey + "] — перейти\n" + unlockedMessage, 0.08f);

        if (Input.GetKeyDown(interactKey))
            UseTransition();
    }

    private void SetupTrigger()
    {
        SphereCollider sphere = GetComponent<SphereCollider>();

        if (sphere == null)
            sphere = gameObject.AddComponent<SphereCollider>();

        sphere.isTrigger = true;
        sphere.radius = interactionRadius;
    }

    private void FindPlayerIfNeeded()
    {
        if (playerTransform != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            playerTransform = player.transform;
    }

    private void BuildVisualMarker()
    {
        visualBuilt = true;

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = "Portal_Marker";
        marker.transform.SetParent(transform);
        marker.transform.localPosition = Vector3.zero;
        marker.transform.localScale = new Vector3(1.3f, 0.08f, 1.3f);

        Collider markerCollider = marker.GetComponent<Collider>();
        if (markerCollider != null)
            Destroy(markerCollider);

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = isUnlocked ? new Color(0.1f, 0.8f, 0.25f, 0.75f) : new Color(0.75f, 0.1f, 0.08f, 0.75f);

        Renderer renderer = marker.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material = mat;
    }

    public void Unlock()
    {
        isUnlocked = true;
        TransitionHintUI.Show(unlockedMessage, 2f);
    }

    public void Lock()
    {
        isUnlocked = false;
    }

    public void SetUnlocked(bool value)
    {
        isUnlocked = value;
    }

    public bool CanUse()
    {
        return !requireUnlock || isUnlocked;
    }

    public void UseTransition()
    {
        if (!CanUse())
        {
            TransitionHintUI.Show(lockedMessage, 2f);
            return;
        }

        if (onTransitionUsed != null)
            onTransitionUsed.Invoke();

        if (loadSceneOnUse && !string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("[LevelTransitionPortal] Переход сработал. Сцена: " + nextSceneName + ". loadSceneOnUse=false");
            TransitionHintUI.Show("Переход готов. Загрузка сцены пока выключена.", 2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = CanUse() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
