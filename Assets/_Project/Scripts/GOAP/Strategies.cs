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

    readonly CountdownTimer attackAnimationTimer;
    readonly CountdownTimer waitTimer;
    readonly AnimationController animations;

    private bool isAttacking = true;
    private int attackCount = 0;
    private const int MAX_ATTACKS = 3;

    public AttackStrategy(AnimationController animations, ScriptableFloatValue boredom, float funFactor = 35) {
        this.animations = animations;
        attackAnimationTimer = new CountdownTimer(2.2f);

        attackAnimationTimer.OnTimerStart += () => {
            isAttacking = true;
            Complete = false;
        };
        attackAnimationTimer.OnTimerStop += () => {
            attackCount++;

            if (attackCount < MAX_ATTACKS) {
                // Starte den nächsten Angriff, bis MAX_ATTACKS erreicht ist
                animations.Attack();
                boredom.Value -= funFactor;
                attackAnimationTimer.Start();
            }
            else {
                // Nach MAX_ATTACKS Angriffen: Wechsle zum Warten
                isAttacking = false;
                animations.Locomotion();
                waitTimer.Start();
                attackCount = 0; // Zurücksetzen für den nächsten Angriffszyklus
            }
        };

        waitTimer = new CountdownTimer(1f);
        waitTimer.OnTimerStart += () => Complete = false;
        waitTimer.OnTimerStop += () => Complete = true;
    }

    public void Start() {
        attackAnimationTimer.Start();
        animations.Attack();
    }

    public void Update(float deltaTime) {
        if (isAttacking) attackAnimationTimer.Tick(deltaTime);
        else waitTimer.Tick(deltaTime);
    }

    public void Stop() {
        // Aufräumarbeiten, wenn die Strategy vorzeitig beendet wird
        if (isAttacking) {
            attackAnimationTimer.Stop();
        }
        else {
            waitTimer.Stop();
        }

        attackCount = 0; // Zurücksetzen des Angriffszählers
        Complete = true;
    }
}

public class MoveStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly AnimationController animations;
    readonly Func<Vector3> destination;
    readonly float stoppingDistance;

    public bool CanPerform => !Complete;
    public bool Complete   => agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending;

    public MoveStrategy(NavMeshAgent agent, AnimationController animations, Func<Vector3> destination,
        float                        stoppingDistance = 1f) {
        this.agent = agent;
        this.animations = animations;
        this.destination = destination;
        this.stoppingDistance = stoppingDistance;
    }

    public void Start() {
        animations.Locomotion();
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(destination());
    }

    public void Stop() {
        agent.transform.LookAt(destination());
        agent.ResetPath();
    }
}

public class WanderStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly float wanderRadius;
    private readonly int wanderSteps;
    private int m_wanderStepsCounter;


    public bool CanPerform => !Complete;

    public bool Complete =>
        agent.remainingDistance <= 2f &&
        !agent.pathPending; // Die letzte Condition funktioniert nicht: && m_wanderStepsCounter >= wanderSteps

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
        sleepAnimationTimer.OnTimerStart += () => { Complete = false; };
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

public class EatAndWaitStrategy : IActionStrategy
{
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
        animations.Eat();
        eatAnimationTimer.Start();
    }

    public void Update(float deltaTime) {
        if (m_isEating) {
            eatAnimationTimer.Tick(deltaTime);
        }
        else {
            waitTimer.Tick(deltaTime);
        }
    }

    public void Stop() {
        // Aufräumarbeiten, wenn die Strategy vorzeitig beendet wird
        if (m_isEating) {
            eatAnimationTimer.Stop();
        }
        else {
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

public class FetchBallStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete   { get; private set; }

    private readonly NavMeshAgent agent;
    private readonly AnimationController animations;
    private readonly GameObject ball;
    private readonly Transform playerTransform;
    private readonly Transform objectGrabPoint;
    private readonly ScriptableBoolValue ballThrown;
    private readonly ScriptableBoolValue ballInHand;
    private readonly float pickupRange;
    private readonly float dropRange;

    private enum FetchState
    {
        MovingToBall,
        PickingUpBall,
        ReturningToPlayer,
        DroppingBall,
        Completed
    }

    private FetchState currentState;
    private CountdownTimer pickupAnimationTimer;
    private CountdownTimer dropAnimationTimer;

    public FetchBallStrategy(NavMeshAgent agent, AnimationController animations, GameObject ball,
        Transform playerTransform,
        Transform objectGrabPoint, ScriptableBoolValue ballInHand, ScriptableBoolValue ballThrown,
        float pickupRange = 2f, float dropRange = 2f) {
        this.agent = agent;
        this.animations = animations;
        this.ball = ball;
        this.playerTransform = playerTransform;
        this.objectGrabPoint = objectGrabPoint;
        this.pickupRange = pickupRange;
        this.dropRange = dropRange;
        this.ballInHand = ballInHand;
        this.ballThrown = ballThrown;

        // Timer für die Aufheb-Animation
        pickupAnimationTimer = new CountdownTimer(2.4f);
        pickupAnimationTimer.OnTimerStart += () => Complete = false;
        pickupAnimationTimer.OnTimerStop += () => {
            animations.Locomotion();
            if (ball.TryGetComponent(out GrabbableObject grabbableObject)) {
                grabbableObject.Grab(objectGrabPoint);
                currentState = FetchState.ReturningToPlayer;
            }
        };

        // Timer für die Ableg-Animation
        dropAnimationTimer = new CountdownTimer(2.4f);
        dropAnimationTimer.OnTimerStart += () => Complete = false;
        dropAnimationTimer.OnTimerStop += () => {
            if (ball.TryGetComponent(out GrabbableObject grabbableObject)) {
                grabbableObject.Drop();
                currentState = FetchState.Completed;
                Complete = true;
            }
        };
    }

    public void Start() {
        Complete = false;

        currentState = FetchState.MovingToBall;
        animations.SetSpeed(agent.velocity.magnitude);
        agent.stoppingDistance = pickupRange - 0.2f;
        agent.SetDestination(ball.transform.position);
    }

    public void Update(float deltaTime) {
        switch (currentState) {
            case FetchState.MovingToBall:
                MoveToBall();
                break;
            case FetchState.PickingUpBall:
                PickUpBall(deltaTime);
                break;
            case FetchState.ReturningToPlayer:
                ReturnToPlayer();
                break;
            case FetchState.DroppingBall:
                DropBall(deltaTime);
                break;
            case FetchState.Completed:
                // Bereits abgeschlossen
                break;
        }
    }

    private void MoveToBall() {
        // Setze Laufanimation
        animations.Locomotion();

        // Überprüfe, ob Ziel erreicht wurde
        if (Vector3.Distance(agent.transform.position, ball.transform.position) <= pickupRange) {
            currentState = FetchState.PickingUpBall;
        }
    }

    private void PickUpBall(float deltaTime) {
        if (!pickupAnimationTimer.IsRunning) {
            animations.Eat(); // Eat als aufheben
            pickupAnimationTimer.Start();
        }

        pickupAnimationTimer.Tick(deltaTime);
    }

    private void ReturnToPlayer() {
        agent.stoppingDistance = dropRange - 0.2f;
        agent.SetDestination(playerTransform.position);

        // // Setze Laufanimation
        // animations.Locomotion();
        // animations.SetSpeed(agent.velocity.magnitude);

        // Überprüfe, ob Ziel erreicht wurde
        if (Vector3.Distance(agent.transform.position, playerTransform.position) <= dropRange) {
            currentState = FetchState.DroppingBall;
        }
    }

    private void DropBall(float deltaTime) {
        if (!dropAnimationTimer.IsRunning) {
            animations.Eat();
            dropAnimationTimer.Start();
            if (ball.TryGetComponent(out GrabbableObject grabbableObject)) {
                grabbableObject.Drop();
                // Explizit ballThrown auf false setzen, um zu verhindern, dass die FetchBall-Aktion sofort wieder ausgeführt wird
                ballThrown.Value = false;
            }
        }

        // Wichtig: Stelle sicher, dass der Ball wirklich abgelegt wird, wenn die Strategie gestoppt wird
        if (currentState == FetchState.ReturningToPlayer || currentState == FetchState.DroppingBall) {
            if (ball.TryGetComponent(out GrabbableObject grabbableObject)) {
                grabbableObject.Drop();
                ballThrown.Value = false;
                ballInHand.Value = false;
            }
        }

        dropAnimationTimer.Tick(deltaTime);
    }

    public void Stop() {
        if (pickupAnimationTimer.IsRunning) {
            pickupAnimationTimer.Stop();
        }

        if (dropAnimationTimer.IsRunning) {
            dropAnimationTimer.Stop();
        }

        animations.Locomotion();
        agent.ResetPath();
        Complete = true;
    }
}

public class SeekAttentionStrategy : IActionStrategy
{
    public bool CanPerform => true; // Agent kann immer schlafen
    public bool Complete   { get; private set; }

    readonly CountdownTimer begAnimationTimer;
    readonly NavMeshAgent navMeshAgent;
    readonly AnimationController animations;

    public SeekAttentionStrategy(NavMeshAgent navMeshAgent, AnimationController animations, Transform playerPos, ScriptableFloatValue boredom, float frust = 2) {
        this.navMeshAgent = navMeshAgent;
        this.animations = animations;

        begAnimationTimer = new CountdownTimer(5f);
        begAnimationTimer.OnTimerStart += () => {
            navMeshAgent.transform.LookAt(playerPos);

            Complete = false;
        };
        begAnimationTimer.OnTimerStop += () => {
            boredom.Value -= frust;
            animations.Locomotion(); // Wechsel zurück zur Locomotion-Animation
            Complete = true;
        };
    }

    public void Start() {
        // Starte die Sleep-Animation
        animations.Beg();
        // Starte den Timer
        begAnimationTimer.Start();
    }

    public void Update(float deltaTime) {
        begAnimationTimer.Tick(deltaTime);
    }

    public void Stop() {
        // Aufräumarbeiten, wenn die Strategy vorzeitig beendet wird
        begAnimationTimer.Stop();
        Complete = true;
    }
}

public class PickUpBallStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete   { get; private set; }

    private readonly NavMeshAgent agent;
    private readonly AnimationController animations;
    private readonly GameObject ball;
    private readonly Transform objectGrabPoint;
    private readonly float pickupRange;

    private CountdownTimer pickupAnimationTimer;

    public PickUpBallStrategy(NavMeshAgent agent,           AnimationController animations, GameObject ball,
        Transform                          objectGrabPoint, float               pickupRange = 2f) {
        this.agent = agent;
        this.animations = animations;
        this.ball = ball;
        this.objectGrabPoint = objectGrabPoint;
        this.pickupRange = pickupRange;

        pickupAnimationTimer = new CountdownTimer(2.4f);
        pickupAnimationTimer.OnTimerStart += () => {
            animations.Eat();
            Complete = false;
        };
        pickupAnimationTimer.OnTimerStop += () => {
            animations.Locomotion();
            if (ball.TryGetComponent(out GrabbableObject grabbableObject)) {
                grabbableObject.Grab(objectGrabPoint);
            }

            Complete = true;
        };
    }

    public void Start() {
        pickupAnimationTimer.Start();
        agent.stoppingDistance = pickupRange - 0.2f;
        agent.SetDestination(ball.transform.position);
    }

    public void Update(float deltaTime) {
        pickupAnimationTimer.Tick(deltaTime);
    }

    public void Stop() {
        pickupAnimationTimer.Stop();
        animations.Locomotion();
        Complete = true;
    }
}

public class DropBallStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete   { get; private set; }

    private readonly NavMeshAgent agent;
    private readonly AnimationController animations;
    private readonly GameObject ball;
    private readonly float dropRange;

    private CountdownTimer dropAnimationTimer;

    public DropBallStrategy(NavMeshAgent agent,           AnimationController animations,   GameObject ball,
        Transform                        objectGrabPoint, ScriptableBoolValue ballReturned, float      dropRange = 2f) {
        this.agent = agent;
        this.animations = animations;
        this.ball = ball;
        this.dropRange = dropRange;

        dropAnimationTimer = new CountdownTimer(2.4f);
        dropAnimationTimer.OnTimerStart += () => {
            animations.Eat();
            Complete = false;
        };
        dropAnimationTimer.OnTimerStop += () => {
            animations.Locomotion();
            if (ball.TryGetComponent(out GrabbableObject grabbableObject)) {
                // Animation Event beim Aufheben und Droppen
                grabbableObject.Grab(objectGrabPoint);
            }

            ballReturned.Value = true;
            Complete = true;
        };
    }

    public void Start() {
        dropAnimationTimer.Start();
        agent.stoppingDistance = dropRange - 0.2f;
        agent.SetDestination(ball.transform.position);
    }

    public void Update(float deltaTime) {
        dropAnimationTimer.Tick(deltaTime);
    }

    public void Stop() {
        dropAnimationTimer.Stop();
        animations.Locomotion();
        Complete = true;
    }
}