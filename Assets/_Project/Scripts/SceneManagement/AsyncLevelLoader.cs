using System;
using System.Collections;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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
    private float duration = 1f;

    [Header("Progress Bar"), SerializeField]
    private Slider progressBar;

    [SerializeField]
    private TMP_Text progressText;
    
    [FoldoutGroup("Events"), SerializeField]
    private UnityEvent onLevelChange;

    public static AsyncLevelLoader Instance;
    
    private const float zero = 0f;
    private const float one = 1f;

    private float target;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }

    public async void LoadScene(int sceneIndex)
    {
        try
        {
            onLevelChange?.Invoke();
            EventSystem.current.SetSelectedGameObject(null);
            var color = loadingScreenImage.color;
            color.a = one;
            loadingScreenImage.color = color;
            loadingScreen.SetActive(true);
            progressBar.gameObject.SetActive(true);
            progressBar.value = zero;
            target = zero;

            await Task.Delay(300);

            var scene = SceneManager.LoadSceneAsync(sceneIndex);
            if (scene == null) return;

            scene.allowSceneActivation = false;

            do
            {
                await Task.Delay(500);
                target = Mathf.Clamp01(scene.progress / 0.9f);
            } while (scene.progress < 0.9f);

            await Task.Delay(2000);

            if (sceneIndex == 0)
            {
                mainMenu.SetActive(true);
                mainMenuImage.SetActive(true);
            }
            else
            {
                mainMenu.SetActive(false);
                mainMenuImage.SetActive(false);
            }

            progressBar.gameObject.SetActive(false);
            FadeOutLoadingScreen();
            scene.allowSceneActivation = true;
        }
        catch (Exception e)
        {
            throw new Exception($"{e}");
        }
    }

    void Update()
    {
        progressBar.value = Mathf.MoveTowards(progressBar.value, target, Time.deltaTime * 0.5f);
        progressText.text = $"{progressBar.value * 100:0}%";
    }

    public void FadeOutLoadingScreen()
    {
        StartCoroutine(FadeOutLoadingScreenCoroutine());
    }

    private IEnumerator FadeOutLoadingScreenCoroutine()
    {
        if (loadingScreenImage == null || duration <= 0f)
            yield break;

        progressBar.gameObject.SetActive(false);

        var color = loadingScreenImage.color;
        color.a = 1f; 
        loadingScreenImage.color = color;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(1f, 0f, t);
            loadingScreenImage.color = color;
            yield return null;
        }

        color.a = 0f;
        loadingScreenImage.color = color;
        loadingScreen.SetActive(false);
    }

    public void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
    }
}