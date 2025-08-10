using UnityEngine;

public class ChangeVolumeSlider : MonoBehaviour
{
    public void SetMasterVolume(float volume) {
        SoundMixerManager.Instance.SetMasterVolume(volume);
    }
    
    public void SetMusicVolume(float volume) {
        SoundMixerManager.Instance.SetMusicVolume(volume);
    }
    
    public void SetSoundFXVolume(float volume) {
        SoundMixerManager.Instance.SetSoundFXVolume(volume);
    }
}
