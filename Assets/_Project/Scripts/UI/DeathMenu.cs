using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// public class DeathMenu : MonoBehaviour
// {
//     public GameObject DeathMenuUI;
//     public GameObject JumpInfo;
//     public GameObject HoldJumpInfo;
//     public GameObject RollInfo;
//     public GameObject InvertGravityInfo;
//     public AnimationController AnimationController;
//
//     private void Update()
//     {
//         if (AnimationController.IsDead)
//             ShowDeathMenu();
//     }
//
//     public void ShowDeathMenu()
//     {
//         Cursor.lockState = CursorLockMode.None;
//         DeathMenuUI.SetActive(true);
//         JumpInfo.SetActive(false);
//         HoldJumpInfo.SetActive(false);
//         RollInfo.SetActive(false);
//         InvertGravityInfo.SetActive(false);
//     }
//
//     public void RestartGame()
//     {
//         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//         Time.timeScale = 1;
//     }
//     public void Quit()
//     {
//         SceneManager.LoadScene("MainMenu");
//         DeathMenuUI.SetActive(false);
//         Time.timeScale = 1f;
//     }
//
//     //IEnumerator ExecuteAfterDelay()
//     //{
//     //    yield return new WaitForSeconds(1f);
//
//     //    ShowDeathMenu();
//     //}
//
//
//     //public void ShowDeathMenuOnDeath()
//     //{
//     //    if (AnimationController.IsDead)
//     //    {
//     //        ShowDeathMenu();
//     //    }
//     //}
// }
