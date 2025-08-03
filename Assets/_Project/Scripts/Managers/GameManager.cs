using System;
using ImprovedTimers;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private float roundDuration;
    
    [HideInInspector]
    public CountdownTimer gameTimer;

    private void Awake() {
        gameTimer = new CountdownTimer(roundDuration);
    }
    
    private void Start() {
        gameTimer.Start();
    }
    
    private void OnDestroy() {
        gameTimer.Stop();
    }
}
