using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseMenuUI;
    
    [SerializeField]
    private GameObject optionsMenuUI;

    private const int mainMenuSceneIndex = 0;
    

    public void Resume() {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        PlayerController.PlayerInput.SwitchCurrentActionMap("Player");
    }

    public void Pause() {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        PlayerController.PlayerInput.SwitchCurrentActionMap("UI");
    }

    public void LoadMainMenu() {
        AsyncLevelLoader.Instance.LoadScene(mainMenuSceneIndex);
    }
}