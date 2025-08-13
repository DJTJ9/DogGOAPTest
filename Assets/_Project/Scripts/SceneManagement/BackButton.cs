using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
   [SerializeField]
   private GameObject mainMenu;
   
   [SerializeField]
   private GameObject pauseMenu;
   
   public void OnBackButtonPressed() {
      if (SceneManager.GetActiveScene().buildIndex == 0) {
         mainMenu.SetActive(true);
      }
      else {
         pauseMenu.SetActive(true);
      }
   }
}
