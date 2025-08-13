using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class FadeEffectManager : MonoBehaviour
{
    public static FadeEffectManager Instance;

    [SerializeField]
    private Image fadeImage;

    [FoldoutGroup("Fade Settings", expanded: false), Range(0.1f, 10f), SerializeField]
    private float fadeOutSpeed = 5f;

    [FoldoutGroup("Fade Settings", expanded: false), Range(0.1f, 10f), SerializeField]
    private float fadeInSpeed = 5f;

    [FoldoutGroup("Fade Settings", expanded: false), SerializeField]
    private Color fadeStartColor;

    public bool IsFadingOut { get; private set; }
    public bool IsFadingIn  { get; private set; }

    private const float zero = 0;
    private const float one = 1;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else {
            Destroy(gameObject);
        }

        fadeStartColor.a = zero;
    }

    private void Update() {
        if (IsFadingOut) {
            if (fadeImage.color.a < one) {
                fadeStartColor.a += Time.deltaTime * fadeOutSpeed;
                fadeImage.color = fadeStartColor;
            }
            else {
                IsFadingOut = false;
            }
        }

        if (IsFadingIn) {
            if (fadeImage.color.a > zero) {
                fadeStartColor.a -= Time.deltaTime * fadeInSpeed;
                fadeImage.color = fadeStartColor;
            }
            else {
                IsFadingIn = false;
            }
        }
    }


    public void StartFadeOut() {
        fadeImage.color = fadeStartColor;
        IsFadingOut = true;
    }

    public void StartFadeIn() {
        if (fadeImage.color.a >= one) {
            fadeImage.color = fadeStartColor;
            IsFadingIn = true;
        }
    }
}