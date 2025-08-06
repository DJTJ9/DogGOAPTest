using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenuManager : MonoBehaviour
{
    [SerializeField]
    private AudioMixer audioMixer;
    public Slider slider;


    //private void Start()
    //{
    //    slider.value = 1f;
    //    SetVolume(slider.value);
    //    slider.onValueChanged.AddListener(SetVolume);
    //}

    public void PlayGame()
    {
        SceneManager.LoadScene("Level1");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", Mathf.Log10(volume) + 20);
    }

    public void SetQuality(int _qualityIndex)
    {
        QualitySettings.SetQualityLevel(_qualityIndex);
    }

    public void SetFullScreen(bool _isFullscreen)
    {
        Screen.fullScreen = _isFullscreen;
    }
}
