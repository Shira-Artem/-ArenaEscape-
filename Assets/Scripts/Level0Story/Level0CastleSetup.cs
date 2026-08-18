using System.Collections;
using UnityEngine;

public class Level0CastleSetup : MonoBehaviour
{
    public static GameObject RuntimePlayer { get; private set; }
    public static bool SetupComplete { get; private set; }

    [Header("Spawn")]
    public bool setupOnStart = false;
    public Vector3 playerStartPosition = new Vector3(0f, 1.2f, -34f);
    public Vector3 playerStartEuler = Vector3.zero;

    [Header("Camera")]
    public Vector3 cameraLocalPosition = new Vector3(0f, 1.62f, 0f);
    public float cameraFov = 78f;
    public float mouseSensitivity = 95f;

    [Header("Movement")]
    public float walkSpeed = 6.2f;
    public float runSpeed = 9.6f;
    public float jumpForce = 7.2f;
    public bool disableHeadBob = true;

    [Header("Cleanup")]
    public bool destroyOldPlayers = true;
    public bool destroyOldCameras = true;
    public bool disableCastleControllers = true;

    private bool isSettingUp;

    private IEnumerator Start()
    {
        if (!setupOnStart)
            yield break;

        yield return null;
        yield return null;
        SetupArenaPlayer();
    }

    [ContextMenu("Setup Arena Player")]
    public GameObject SetupArenaPlayer()
    {
        if (isSettingUp)
            return RuntimePlayer;

        isSettingUp = true;
        SetupComplete = false;

        if (disableCastleControllers)
            DisableCastleInputScripts();

        if (destroyOldPlayers)
            CleanupPlayers();

        if (destroyOldCameras)
            CleanupCameras();

        RuntimePlayer = CreateCleanArenaPlayer();
        SetupManagers();

        MouseLook.Lock();

        SetupComplete = true;
        isSettingUp = false;

        Debug.Log("[Level0CastleSetup] Clean first-person player created.");
        return RuntimePlayer;
    }

    private void DisableCastleInputScripts()
    {
        CastlePlayerController[] controllers = FindObjectsByType<CastlePlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (CastlePlayerController c in controllers)
        {
            if (c != null)
                c.enabled = false;
        }

        CastleCameraLook[] looks = FindObjectsByType<CastleCameraLook>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (CastleCameraLook look in looks)
        {
            if (look != null)
                look.enabled = false;
        }
    }

    private void CleanupPlayers()
    {
        GameObject[] taggedPlayers = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in taggedPlayers)
        {
            if (p == null)
                continue;

            p.SetActive(false);
            Destroy(p);
        }

        GameObject namedPlayer = GameObject.Find("Player");
        if (namedPlayer != null)
        {
            namedPlayer.SetActive(false);
            Destroy(namedPlayer);
        }
    }

    private void CleanupCameras()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Camera cam in cameras)
        {
            if (cam == null)
                continue;

            cam.gameObject.SetActive(false);
            Destroy(cam.gameObject);
        }

        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (AudioListener listener in listeners)
        {
            if (listener != null)
            {
                listener.enabled = false;
                Destroy(listener);
            }
        }
    }

    private GameObject CreateCleanArenaPlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Default");
        player.transform.position = playerStartPosition;
        player.transform.rotation = Quaternion.Euler(playerStartEuler);

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.82f;
        cc.radius = 0.38f;
        cc.center = new Vector3(0f, 0.91f, 0f);
        cc.stepOffset = 0.45f;
        cc.slopeLimit = 55f;
        cc.minMoveDistance = 0f;

        PlayerHealth health = player.AddComponent<PlayerHealth>();
        health.respawnPoint = playerStartPosition;

        PlayerController controller = player.AddComponent<PlayerController>();
        controller.walkSpeed = walkSpeed;
        controller.runSpeed = runSpeed;
        controller.jumpForce = jumpForce;
        controller.headBobEnabled = !disableHeadBob;

        if (player.GetComponent<PauseMenu>() == null)
            player.AddComponent<PauseMenu>();

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(player.transform);
        cameraObject.transform.localPosition = cameraLocalPosition;
        cameraObject.transform.localRotation = Quaternion.identity;

        Camera cam = cameraObject.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.fieldOfView = cameraFov;
        cam.nearClipPlane = 0.05f;
        cam.farClipPlane = 1000f;

        cameraObject.AddComponent<AudioListener>();

        MouseLook look = cameraObject.AddComponent<MouseLook>();
        look.sensitivity = mouseSensitivity;

        CameraShake shake = cameraObject.AddComponent<CameraShake>();
        health.shake = shake;

        WeaponController weaponController = cameraObject.AddComponent<WeaponController>();
        weaponController.enabled = false;

        HideAllPlayerRenderers(player);

        return player;
    }

    private void HideAllPlayerRenderers(GameObject player)
    {
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
                renderer.enabled = false;
        }
    }

    private void SetupManagers()
    {
        if (GameManager.Instance == null)
            new GameObject("GM").AddComponent<GameManager>();

        if (FindFirstObjectByType<FeedbackManager>() == null)
            new GameObject("FB").AddComponent<FeedbackManager>();

        if (FindFirstObjectByType<KingAbility>() == null)
            new GameObject("KingAbility").AddComponent<KingAbility>();

        GameProgressManager.Ensure();

        if (RunScoreManager.Instance == null)
        {
            var sm = new GameObject("RunScoreManager").AddComponent<RunScoreManager>();
            sm.StartRun();
        }

        if (SceneFader.Instance == null)
            new GameObject("SceneFader").AddComponent<SceneFader>();
        SceneFader.FadeIn();
    }
}
