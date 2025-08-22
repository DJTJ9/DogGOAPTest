using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [FoldoutGroup("Settings", expanded: true), SerializeField]
    private float startVolume = 0.69f;


    [FoldoutGroup("Settings", expanded: true), SerializeField]
    private float dimDuration = 1.5f;

    [FoldoutGroup("Audio Clips", expanded: false), SerializeField]
    private List<AudioClip> musicClips;

    private const float fullVolume = 1f;

    private AudioSource audioSource;

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

        audioSource = GetComponent<AudioSource>();
        audioSource.volume = startVolume;
        audioSource.Play();
    }

    private void Update()
    {
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            ChangeMusicClip();
        }
    }

    private AudioClip lastPlayedClip;

    public void ChangeMusicClip()
    {
        if (musicClips.Count == 0)
            return;

        lastPlayedClip = audioSource.clip;

        if (musicClips.Count > 1 && lastPlayedClip != null)
        {
            List<AudioClip> availableClips = new List<AudioClip>(musicClips);
            availableClips.Remove(lastPlayedClip);

            int randomIndex = Random.Range(0, availableClips.Count);
            audioSource.clip = availableClips[randomIndex];
        }
        else
        {
            audioSource.clip = musicClips[Random.Range(0, musicClips.Count)];
        }

        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    public void DimMusic()
    {
        StartCoroutine(DimMusicCoroutine());
    }

    public void UnDimMusic()
    {
        audioSource.volume = fullVolume;
    }

    private IEnumerator DimMusicCoroutine()
    {
        float currentVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < dimDuration)
        {
            audioSource.volume = Mathf.Lerp(currentVolume, startVolume, elapsedTime / dimDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = startVolume;
    }

    private IEnumerator UnDimMusicCoroutine()
    {
        float currentVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < dimDuration)
        {
            audioSource.volume = Mathf.Lerp(currentVolume, fullVolume, elapsedTime / dimDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = fullVolume;
        ;
    }
}