using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    [SerializeField]
    private GameObject mainMenuUI;

    [SerializeField]
    private GameObject mainMenuBackgroundImage;

    [SerializeField]
    private GameObject pauseMenuUI;

    [SerializeField]
    private GameObject optionsMenuUI;

    [SerializeField]
    private AudioMixer audioMixer;

    [FoldoutGroup("Events", expanded: true), SerializeField]
    private UnityEvent onRestart;

    [FoldoutGroup("Events", expanded: true), SerializeField]
    private UnityEvent onPause;

    [FoldoutGroup("Events", expanded: true), SerializeField]
    private UnityEvent onUnpause;

    private const int mainMenuSceneIndex = 0;
    private const int startLevelSceneIndex = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayGame()
    {
        AsyncLevelLoader.Instance.LoadScene(startLevelSceneIndex);
        mainMenuUI.SetActive(false);
        mainMenuBackgroundImage.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        // PlayerController.PlayerInput.SwitchCurrentActionMap("Player");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        onRestart.Invoke();

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        onRestart.Invoke();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Pause()
    {
        // onPause.Invoke();
        // pauseMenuUI.SetActive(true);
        // Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        PlayerController.PlayerInput.SwitchCurrentActionMap("UI");
    }

    public void Unpause()
    {
        // onUnpause.Invoke();
        // pauseMenuUI.SetActive(false);
        // optionsMenuUI.SetActive(false);
        // Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        PlayerController.PlayerInput.SwitchCurrentActionMap("Player");
    }

    public void LoadMainMenu()
    {
        AsyncLevelLoader.Instance.LoadScene(mainMenuSceneIndex);
        // mainMenuUI.SetActive(true);
        // mainMenuBackgroundImage.SetActive(true);
        Time.timeScale = 1f;
        // Cursor.lockState = CursorLockMode.None;
        // PlayerController.PlayerInput.SwitchCurrentActionMap("UI");
    }

    public void SetQuality(int _qualityIndex)
    {
        QualitySettings.SetQualityLevel(_qualityIndex);
    }

    public void SetFullScreen(bool _isFullscreen)
    {
        Screen.fullScreen = _isFullscreen;
    }

    public void SetCursorLock(bool _isCursorLocked)
    {
        Cursor.lockState = _isCursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
    }
}