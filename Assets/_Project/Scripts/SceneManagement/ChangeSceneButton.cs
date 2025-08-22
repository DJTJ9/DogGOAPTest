using UnityEngine;

public class ChangeSceneButton : MonoBehaviour
{
    private const float zero = 0f;
    private const float one = 1f;

    public void ChangeScene(int sceneIndex)
    {
        AsyncLevelLoader.Instance.LoadScene(sceneIndex);
    }

    public void SetTimeScaleToOne()
    {
        AsyncLevelLoader.Instance.SetTimeScale(one);
    }

    public void SetTimeScaleToZero()
    {
        AsyncLevelLoader.Instance.SetTimeScale(zero);
    }

    public void SetMasterVolumeToOne()
    {
        SoundMixerManager.Instance.SetMasterVolume(one);
    }

    public void SetMasterVolumeToZero()
    {
        SoundMixerManager.Instance.SetMasterVolume(zero);
    }
}