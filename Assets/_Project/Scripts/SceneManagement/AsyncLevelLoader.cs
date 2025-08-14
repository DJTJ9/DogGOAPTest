using System;
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

    [SerializeField]
    private GameObject mainMenuImage;

    [SerializeField]
    private GameObject loadingScreen;

    [Header("Fade Image"), SerializeField]
    private Image loadingScreenImage;

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
        try {
            onLevelChange?.Invoke();
            var color = loadingScreenImage.color;
            color.a = 1f;
            // FadeEffectManager.Instance.StartFadeOut();
            loadingScreenImage.color = color;
            loadingScreen.SetActive(true);
            progressBar.gameObject.SetActive(true);
            progressBar.value = 0;
            target = 0;
            
            await Task.Delay(300);

            var scene = SceneManager.LoadSceneAsync(sceneIndex);
            if (scene == null) return;

            scene.allowSceneActivation = false;

            do {
                await Task.Delay(500);
                target = Mathf.Clamp01(scene.progress / 0.9f);
            } while (scene.progress < 0.9f);

            await Task.Delay(1000);

            if (sceneIndex == 0) {
                mainMenu.SetActive(true);
                mainMenuImage.SetActive(true);
                duration = 1f;
            }
            else {
                mainMenu.SetActive(false);
                mainMenuImage.SetActive(false);
                duration = 2f;
            }

            progressBar.gameObject.SetActive(false);
            // FadeEffectManager.Instance.StartFadeIn();
            FadeOutLoadingScreen();
            scene.allowSceneActivation = true;
            MusicManager.Instance.ChangeMusicClip();
            
            // await Task.Delay(2000);
            // loadingScreen.SetActive(false);
        }
        catch (Exception e) {
            throw new Exception($"{e}");
        }
    }

    void Update() {
        progressBar.value = Mathf.MoveTowards(progressBar.value, target, Time.deltaTime * 0.5f);
        progressText.text = $"{progressBar.value * 100:0}%";
    }

    public void FadeOutLoadingScreen() {
        StartCoroutine(FadeOutLoadingScreenCoroutine());
    }

    private IEnumerator FadeOutLoadingScreenCoroutine() {
        // FadeEffectManager.Instance.StartFadeOut();
        //
        // while (FadeEffectManager.Instance.IsFadingOut) {
        //     yield return null;
        // }
        
        
        if (loadingScreenImage == null || duration <= 0f)
            yield break;
        
        progressBar.gameObject.SetActive(false);
        
        // Von 255 (1.0) nach 0 (0.0) ausblenden
        var color = loadingScreenImage.color;
        color.a = 1f; // Start bei 255/volle Sichtbarkeit
        loadingScreenImage.color = color;
        
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(1f, 0f, t);
            loadingScreenImage.color = color;
            yield return null;
        }
        
        // Abschluss sicherstellen
        color.a = 0f;
        loadingScreenImage.color = color;
        loadingScreen.SetActive(false);
    }

    public void SetTimeScale(float timeScale) {
        Time.timeScale = timeScale;
    }
}