using UnityEngine;
using UnityEngine.Audio;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    
    public static SoundMixerManager Instance;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this);
        } else {
            Destroy(gameObject);
        }
    }
    
    public void SetMasterVolume(float volume) {
        float sliderVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(sliderVolume) * 20f);
    }
    
    public void SetMusicVolume(float volume) {
        float sliderVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(sliderVolume) * 20f);
    }
    
    public void SetSoundFXVolume(float volume) {
        float sliderVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        audioMixer.SetFloat("SoundFXVolume", Mathf.Log10(sliderVolume) * 20f);   
    }
}