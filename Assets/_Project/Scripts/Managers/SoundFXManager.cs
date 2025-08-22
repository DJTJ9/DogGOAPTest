using System;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;

    [SerializeField]
    private AudioSource soundFXObject;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, spawnTransform.rotation);

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();

        float clipDuration = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipDuration);
    }

    public void PlaySoundFXWithFixedDurationClip(AudioClip audioClip, Transform spawnTransform, float volume, float duration)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, spawnTransform.rotation);

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();

        Destroy(audioSource.gameObject, duration);
    }

    public void PlayRandomSoundFXClip(AudioClip[] audioClips, Transform spawnTransform, float volume)
    {
        int randomIndex = UnityEngine.Random.Range(0, audioClips.Length);

        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, spawnTransform.rotation);

        audioSource.clip = audioClips[randomIndex];
        audioSource.volume = volume;
        audioSource.Play();

        float clipDuration = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipDuration);
    }
}