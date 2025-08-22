using System;
using ImprovedTimers;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private float roundDuration;

    [HideInInspector]
    public CountdownTimer GameTimer;

    private void Awake()
    {
        GameTimer = new CountdownTimer(roundDuration);
    }

    private void Start()
    {
        GameTimer.Start();
    }

    private void OnDestroy()
    {
        GameTimer.Stop();
    }
}