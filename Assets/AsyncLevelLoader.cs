using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AsyncLevelLoader : MonoBehaviour
{
    [FoldoutGroup("Menu Screens", expanded: false), SerializeField]
    private GameObject mainMenu;
    
    [FoldoutGroup("Menu Screens", expanded: false), SerializeField]
    private GameObject loadingScreen;
    
    [FoldoutGroup("Progress Bar", expanded: false), SerializeField]
    private Slider progressBar;

    [FoldoutGroup("Settings", expanded: false), SerializeField]
    private float minLoadingScreenTime = 5f;
    
    [FoldoutGroup("Events"), SerializeField]
    private UnityEvent onLevelLoaded;
    
    public void LoadLevel(string levelName)
    {
        if (mainMenu != null) mainMenu.SetActive(false);
        if (loadingScreen != null) loadingScreen.SetActive(true);
        StartCoroutine(LoadLevelAsync(levelName));
    }

    private IEnumerator LoadLevelAsync(string levelName) {
        float elapsed = 0f;
        AsyncOperation asyncLoadOperation = SceneManager.LoadSceneAsync(levelName);
        asyncLoadOperation.allowSceneActivation = false;

        // Laden bis 90% (Unity hält bei ~0.9, bis allowSceneActivation true wird)
        while (asyncLoadOperation.progress < 0.9f) {
            if (progressBar != null) {
                float progress = Mathf.Clamp01(asyncLoadOperation.progress / 0.9f);
                progressBar.value = progress;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Fortschritt voll anzeigen, während wir auf die Mindestzeit warten
        while (elapsed < minLoadingScreenTime) {
            if (progressBar != null)
                progressBar.value = 1f;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Jetzt Szene aktivieren
        asyncLoadOperation.allowSceneActivation = true;

        // Warten, bis die Aktivierung abgeschlossen ist
        while (!asyncLoadOperation.isDone)
            yield return null;

        if (loadingScreen != null) loadingScreen.SetActive(false);
        onLevelLoaded?.Invoke();
    }
}
