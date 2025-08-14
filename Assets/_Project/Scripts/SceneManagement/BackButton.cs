using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenu;

    [SerializeField]
    private GameObject pauseMenu;

    [SerializeField]
    private GameObject optionsBubble;

    public void OnBackButtonPressed() {
        if (SceneManager.GetActiveScene().buildIndex == 0) {
            mainMenu.SetActive(true);
            optionsBubble.SetActive(false);
        }
        else {
            pauseMenu.SetActive(true);
        }
    }
}