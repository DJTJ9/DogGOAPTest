using System;
using ImprovedTimers;
using UnityEngine;

public class EatAndWaitStrategy : IActionStrategy {
    public bool CanPerform => true; // Agent kann immer essen
    public bool Complete { get; private set; }

    readonly CountdownTimer eatAnimationTimer;
    readonly CountdownTimer waitTimer;
    readonly AnimationController animations;
    private bool isEating = true;

    public EatAndWaitStrategy(AnimationController animations) {
        this.animations = animations;

        // Timer für die Eat-Animation
        float eatDuration = animations.GetAnimationLength(animations.eatClip);
        eatAnimationTimer = new CountdownTimer(5.7f);
        eatAnimationTimer.OnTimerStart += () => {
            isEating = true;
            Complete = false;
        };
        eatAnimationTimer.OnTimerStop += () => {
            isEating = false;
            animations.Locomotion();
            waitTimer.Start(); // Starte den Wait-Timer nach dem Essen
        };

        // Timer für die Wartezeit nach dem Essen
        waitTimer = new CountdownTimer(1f);
        waitTimer.OnTimerStart += () => Complete = false;
        waitTimer.OnTimerStop += () => Complete = true;
    }

    public void Start() {
        // Starte die Animation
        animations.Eat();
        // Starte den Timer für die Animation
        eatAnimationTimer.Start();
    }

    public void Update(float deltaTime) {
        if (isEating) {
            eatAnimationTimer.Tick(deltaTime);
        } else {
            waitTimer.Tick(deltaTime);
        }
    }

    public void Stop() {
        // Aufräumarbeiten, wenn die Strategy vorzeitig beendet wird
        if (isEating) {
            eatAnimationTimer.Stop();
        } else {
            waitTimer.Stop();
        }
        Complete = true;
    }
}
