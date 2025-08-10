using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class DogSoundManager : MonoBehaviour
{
    [SerializeField]
    private DogSO dog;

    [FoldoutGroup("Audio"), SerializeField]
    private AudioClip barking1;
    
    [FoldoutGroup("Audio"), SerializeField]
    private AudioClip barking2;
    
    [FoldoutGroup("Audio"), SerializeField]
    private AudioClip barking3;

    public void PlayBarkingSoundOnLowStat(int barkSound) {
        switch (barkSound) {
            case 1:
                SoundFXManager.Instance.PlaySoundFXWithFixedDurationClip(barking1, transform, 1f, 0.5f);
                break;
            case 2:
                SoundFXManager.Instance.PlaySoundFXWithFixedDurationClip(barking2, transform, 1f, 0.5f);
                break;
            case 3:
                SoundFXManager.Instance.PlaySoundFXWithFixedDurationClip(barking3, transform, 1f, 0.5f);
                break;
        }
    }
}