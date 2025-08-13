using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    
    [FoldoutGroup("Slider"), SerializeField]
    private Slider masterVolumeSlider;
    
    [FoldoutGroup("Slider"), SerializeField]
    private Slider musicVolumeSlider;
    
    [FoldoutGroup("Slider"), SerializeField]
    private Slider soundFXVolumeSlider;

    private const float minDb = -80f;
    private const float maxDb = 0f;
    
    public static SoundMixerManager Instance;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this);
        } else {
            Destroy(gameObject);
        }
    }
    
    private void Start() {
        // masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        // masterVolumeSlider.value = 1f;
        // musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        // musicVolumeSlider.value = 1f;
        // soundFXVolumeSlider.onValueChanged.AddListener(SetSoundFXVolume);
        // soundFXVolumeSlider.value = 1f;
    }
    
    public void SetMasterVolume(float volume) {
        // var db = masterVolumeSlider.value <= 0.0001f ? minDb : Mathf.Lerp(minDb, maxDb, Mathf.Log10(volume) / Mathf.Log10(1f));
        
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20f);
    }
    
    public void SetMusicVolume(float volume) {
        // var db = musicVolumeSlider.value <= 0.0001f ? minDb : Mathf.Lerp(minDb, maxDb, Mathf.Log10(volume) / Mathf.Log10(1f));
        
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20f);
    }
    
    public void SetSoundFXVolume(float volume) {
        // var db = soundFXVolumeSlider.value <= 0.0001f ? minDb : Mathf.Lerp(minDb, maxDb, Mathf.Log10(volume) / Mathf.Log10(1f));
        
        audioMixer.SetFloat("SoundFXVolume", Mathf.Log10(volume) * 20f);   
    }
}