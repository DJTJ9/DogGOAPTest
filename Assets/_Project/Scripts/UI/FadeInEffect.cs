using System;
using UnityEngine;
using UnityEngine.UI;

public class FadeInEffect : MonoBehaviour
{
    private void Awake() {
        if (AsyncLevelLoader.Instance == null) return;
        AsyncLevelLoader.Instance.FadeOutLoadingScreen();
    }
}
