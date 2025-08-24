using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.Events;

public class OptionsMenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseMenuUI;

    [SerializeField]
    private GameObject optionsMenuUI;

    [SerializeField]
    private AudioMixer audioMixer;

    [SerializeField]
    private UnityEvent onRestart;

    private const int mainMenuSceneIndex = 0;
    private const int startLevelSceneIndex = 1;

    public void PlayGame()
    {
        AsyncLevelLoader.Instance.LoadScene(startLevelSceneIndex);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        onRestart.Invoke();
    }

    public void BackToMainMenu()
    {
        AsyncLevelLoader.Instance.LoadScene(mainMenuSceneIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetQuality(int _qualityIndex)
    {
        QualitySettings.SetQualityLevel(_qualityIndex);
    }

    public void SetFullScreen(bool _isFullscreen)
    {
        Screen.fullScreen = _isFullscreen;
    }
}