using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class DogSoundManager : MonoBehaviour
{
    [SerializeField]
    private DogSO dog;

    [FoldoutGroup("Audio"), SerializeField]
    private AudioClip Barking;

    private void Update() {
        PlayBarkingSoundOnLowStat();
    }

    private void PlayBarkingSoundOnLowStat() {
        if (Input.GetKeyDown(KeyCode.G)) {
            if (dog.Stamina <= 30f || dog.Satiety <= 30f || dog.Hydration <= 30f || dog.Fun <= 30f) {
                SoundFXManager.Instance.PlaySoundFXWithFixedDurationClip(Barking, transform, 1f, 0.5f);
            }
        }
    }
}