using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource _musicAudioSource;

    void Awake() {
        OnGameStateChanged(GameStateManager.Instance.CurrentGameState);
    }

    void OnEnable() {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    void OnDisable() {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState newGameState) {
        if (newGameState == GameState.Gameplay) {
            _musicAudioSource.UnPause();
        }
        else {
            _musicAudioSource.Pause();
        }
    }
}
