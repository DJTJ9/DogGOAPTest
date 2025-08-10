using System.Collections;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AsyncLevelLoader : MonoBehaviour
{
    [Header("Menu Screens"), SerializeField]
    private GameObject mainMenu;
    
    [Header("Menu Screens"), SerializeField]
    private GameObject loadingScreen;
    
    [Header("Fade Image"), SerializeField]
    private Image fadeImage;
    
    [SerializeField]
    private float duration = 5f;
    
    [Header("Progress Bar"), SerializeField]
    private Slider progressBar;
    [SerializeField]
    private TMP_Text progressText;
    
    [FoldoutGroup("Events"), SerializeField]
    private UnityEvent onLevelChange;

    public static AsyncLevelLoader Instance;

    private float target;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }

    public async void LoadScene(int sceneIndex) {
        onLevelChange?.Invoke();
        var color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;
        progressBar.gameObject.SetActive(true);
        progressBar.value = 0;
        target = 0;
        
        var scene = SceneManager.LoadSceneAsync(sceneIndex);
        scene.allowSceneActivation = false;
        loadingScreen.SetActive(true);

        do {
            await Task.Delay(500);
            target = Mathf.Clamp01(scene.progress / 0.9f);
        } while (scene.progress < 0.9f);
        
        await Task.Delay(2000);
        
        if (sceneIndex == 0) mainMenu.SetActive(true);
        else mainMenu.SetActive(false);
        
        // loadingScreen.SetActive(false);
        scene.allowSceneActivation = true;
    }
    
    void Update() {
        progressBar.value = Mathf.MoveTowards(progressBar.value, target, Time.deltaTime * 0.5f);
        progressText.text = $"{progressBar.value * 100:0}%";
    }

    public void FadeOutLoadingScreen() {
        StartCoroutine(FadeOutLoadingScreenCoroutine());
    }

    private IEnumerator FadeOutLoadingScreenCoroutine() {
        {
            if (fadeImage == null || duration <= 0f)
                yield break;
            
            progressBar.gameObject.SetActive(false);

            // Von 255 (1.0) nach 0 (0.0) ausblenden
            var color = fadeImage.color;
            color.a = 1f;               // Start bei 255/volle Sichtbarkeit
            fadeImage.color = color;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                color.a = Mathf.Lerp(1f, 0f, t);
                fadeImage.color = color;
                yield return null;
            }

            // Abschluss sicherstellen
            color.a = 0f;
            fadeImage.color = color;
        }
    }
    
    public void SetTimeScale(float timeScale) {
        Time.timeScale = timeScale;
    }
}
