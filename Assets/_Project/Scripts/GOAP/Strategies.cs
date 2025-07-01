using System;
using ImprovedTimers;
using ScriptableValues;
using UnityEngine;
using UnityEngine.AI;

// TODO Migrate Strategies, Beliefs, Actions and Goals to Scriptable Objects and create Node Editor for them
public interface IActionStrategy
{
    bool CanPerform { get; }
    bool Complete   { get; }

    void Start() {
        // noop
    }

    void Update(float deltaTime) {
        // noop
    }

    void Stop() {
        // noop
    }
}

public class AttackStrategy : IActionStrategy
{
    public bool CanPerform => true; // Agent can always attack
    public bool Complete   { get; private set; }

    readonly CountdownTimer timer;
    readonly AnimationController animations;

    public AttackStrategy(AnimationController animations, ScriptableFloatValue boredom, float funFactor = 80) {
        this.animations = animations;
        timer = new CountdownTimer(animations.GetAnimationLength(animations.attackClip));
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => {
            animations.Locomotion();
            boredom.Value -= funFactor;
            Complete = true;
        };
    }

    public void Start() {
        timer.Start();
        animations.Attack();
    }

    public void Update(float deltaTime) => timer.Tick(deltaTime);
}

public class MoveStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly Func<Vector3> destination;
    readonly float stoppingDistance;

    public bool CanPerform => !Complete;
    public bool Complete   => agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending;

    public MoveStrategy(NavMeshAgent agent, Func<Vector3> destination, float stoppingDistance = 1f) {
        this.agent = agent;
        this.destination = destination;
        this.stoppingDistance = stoppingDistance;
    }

    public void Start() {
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(destination());
    }

    public void Stop() {
        agent.transform.LookAt(destination());
        agent.ResetPath();
    }
}

public class WanderStrategy : IActionStrategy {
    readonly NavMeshAgent agent;
    readonly float wanderRadius;
    private readonly int wanderSteps;
    private int m_wanderStepsCounter;
    
    
    public bool CanPerform => !Complete;
    public bool Complete   => agent.remainingDistance <= 2f && !agent.pathPending; // Die letzte Condition funktioniert nicht: && m_wanderStepsCounter >= wanderSteps
    
    public WanderStrategy(NavMeshAgent agent, float wanderRadius, int wanderSteps = 5) {
        this.agent = agent;
        this.wanderRadius = wanderRadius;
        this.wanderSteps = wanderSteps;
        m_wanderStepsCounter = 0;
    }

    public void Start() {
        for (int i = 0; i < wanderSteps; i++) {
            Vector3 randomDirection = (UnityEngine.Random.insideUnitSphere * wanderRadius).With(y: 0);
            NavMeshHit hit;

            if (NavMesh.SamplePosition(agent.transform.position + randomDirection, out hit, wanderRadius, 1)) {
                agent.SetDestination(hit.position);
                m_wanderStepsCounter++;
                return;
            }
        }
    }
}


public class IdleStrategy : IActionStrategy
{
    public bool CanPerform => true; // Agent can always Idle
    public bool Complete   { get; private set; }

    readonly CountdownTimer timer;

    public IdleStrategy(float duration, ScriptableFloatValue stamina, float refill = 10) {
        timer = new CountdownTimer(duration);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => {
            stamina.Value += refill;
            Complete = true;
        };
    }

    public void Start()                 => timer.Start();
    public void Update(float deltaTime) => timer.Tick(deltaTime);
}

public class SleepAndWaitStrategy : IActionStrategy
{
    public bool CanPerform => true; // Agent kann immer schlafen
    public bool Complete   { get; private set; }

    readonly CountdownTimer sleepAnimationTimer;
    readonly AnimationController animations;

    public SleepAndWaitStrategy(AnimationController animations, ScriptableFloatValue stamina, float refill = 100) {
        this.animations = animations;

        // Timer für die Sleep-Animation
        sleepAnimationTimer = new CountdownTimer(10f); // 10 Sekunden Dauer
        sleepAnimationTimer.OnTimerStart += () => {
            Complete = false;
        };
        sleepAnimationTimer.OnTimerStop += () => {
            stamina.Value += refill;
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

public class EatAndWaitStrategy : IActionStrategy {
    public bool CanPerform => true; // Agent kann immer essen
    public bool Complete   { get; private set; }

    readonly CountdownTimer eatAnimationTimer;
    readonly CountdownTimer waitTimer;
    readonly AnimationController animations;
    
    private bool m_isEating = true;

    public EatAndWaitStrategy(AnimationController animations, ScriptableFloatValue hunger, float saturation = 69f) {
        this.animations = animations;

        // Timer für die Eat-Animation
        float eatDuration = animations.GetAnimationLength(animations.eatClip);
        eatAnimationTimer = new CountdownTimer(5.7f);
        eatAnimationTimer.OnTimerStart += () => {
            m_isEating = true;
            Complete = false;
        };
        eatAnimationTimer.OnTimerStop += () => {
            hunger.Value += saturation;
            m_isEating = false;
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
        if (m_isEating) {
            eatAnimationTimer.Tick(deltaTime);
        } else {
            waitTimer.Tick(deltaTime);
        }
    }

    public void Stop() {
        // Aufräumarbeiten, wenn die Strategy vorzeitig beendet wird
        if (m_isEating) {
            eatAnimationTimer.Stop();
        } else {
            waitTimer.Stop();
        }
        Complete = true;
    }
}

public class DrinkAndWaitStrategy : IActionStrategy
{
    public bool CanPerform => true; // Agent kann immer essen
    public bool Complete   { get; private set; }

    readonly CountdownTimer drinkAnimationTimer;
    readonly CountdownTimer waitTimer;
    readonly AnimationController animations;
    private bool isDrinking = true;

    public DrinkAndWaitStrategy(AnimationController animations, ScriptableFloatValue thirst, float hydration = 69f) {
        this.animations = animations;

        // Timer für die Eat-Animation
        float drinkDuration = animations.GetAnimationLength(animations.drinkClip);
        drinkAnimationTimer = new CountdownTimer(5f);
        drinkAnimationTimer.OnTimerStart += () => {
            isDrinking = true;
            Complete = false;
        };
        drinkAnimationTimer.OnTimerStop += () => {
            thirst.Value += hydration;
            isDrinking = false;
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
        animations.Drink();
        // Starte den Timer für die Animation
        drinkAnimationTimer.Start();
    }

    public void Update(float deltaTime) {
        if (isDrinking) {
            drinkAnimationTimer.Tick(deltaTime);
        }
        else {
            waitTimer.Tick(deltaTime);
        }
    }

    public void Stop() {
        // Aufräumarbeiten, wenn die Strategy vorzeitig beendet wird
        if (isDrinking) {
            drinkAnimationTimer.Stop();
        }
        else {
            waitTimer.Stop();
        }

        Complete = true;
    }
}