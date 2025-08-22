using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundMixerManager : MonoBehaviour
{
    public static SoundMixerManager Instance;
    
    [SerializeField] 
    private AudioMixer audioMixer;
    
    [FoldoutGroup("Slider"), SerializeField]
    private Slider masterVolumeSlider;
    
    [FoldoutGroup("Slider"), SerializeField]
    private Slider musicVolumeSlider;
    
    [FoldoutGroup("Slider"), SerializeField]
    private Slider soundFXVolumeSlider;
    
    private void Awake() 
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this);
        } else {
            Destroy(gameObject);
        }
    }
   
    public void SetMasterVolume(float volume) 
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20f);
    }
    
    public void SetMusicVolume(float volume) 
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20f);
    }
    
    public void SetSoundFXVolume(float volume) 
    {
        audioMixer.SetFloat("SoundFXVolume", Mathf.Log10(volume) * 20f);   
    }
}