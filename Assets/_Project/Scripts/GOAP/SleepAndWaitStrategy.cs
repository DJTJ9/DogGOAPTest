using System;
using ImprovedTimers;
using UnityEngine;

public class SleepAndWaitStrategy : IActionStrategy
{
    public bool CanPerform => true; // Agent kann immer schlafen
    public bool Complete   { get; private set; }

    readonly CountdownTimer sleepAnimationTimer;
    readonly AnimationController animations;

    public SleepAndWaitStrategy(AnimationController animations) {
        this.animations = animations;

        // Timer für die Sleep-Animation
        sleepAnimationTimer = new CountdownTimer(10f); // 10 Sekunden Dauer
        sleepAnimationTimer.OnTimerStart += () => {
            Complete = false;
        };
        sleepAnimationTimer.OnTimerStop += () => {
            animations.Locomotion(); // Wechsel zurück zur Locomotion-Animation
            Complete = true;
        };
    }

    public void Start() {
        // Starte die Sleep-Animation
        animations.Sleep();
        // Starte den Timer
        sleepAnimationTimer.Start();
    }

    public void Update(float deltaTime) {
        sleepAnimationTimer.Tick(deltaTime);
    }

    public void Stop() {
        // Aufräumarbeiten, wenn die Strategy vorzeitig beendet wird
        sleepAnimationTimer.Stop();
        Complete = true;
    }
}
