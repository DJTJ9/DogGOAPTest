using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenuManager : MonoBehaviour
{
    // [SerializeField]
    // private GameObject mainMenuUI;
    
    [SerializeField]
    private GameObject pauseMenuUI;
    
    [SerializeField]
    private GameObject optionsMenuUI;
    
    [SerializeField]
    private AudioMixer audioMixer;

    public void PlayGame()
    {
        SceneManager.LoadScene("StartLevel");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }
    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", Mathf.Log10(volume) + 20);
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
