using System;
using ImprovedTimers;
using ScriptableValues;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
    public bool CanPerform => true;
    public bool Complete   { get; private set; }

    readonly CountdownTimer attackAnimationTimer;
    readonly AnimationController animations;
    private readonly float hitDamage = 25f;

    public AttackStrategy(AnimationController animations, DogSO dog, Obstacle obstacle, float funFactor = 100) {
        this.animations = animations;
        attackAnimationTimer = new CountdownTimer(7f);

        attackAnimationTimer.OnTimerStart += () => { Complete = false; };
        attackAnimationTimer.OnTimerStop += () => {
            dog.Fun += funFactor;
            obstacle.TakeDamage(hitDamage);
            Complete = true;
        };
    }

    public void Start() {
        attackAnimationTimer.Start();
        animations.StartDogAction(AnimationActionType.Shake);
    }

    public void Update(float deltaTime) {
        attackAnimationTimer.Tick(deltaTime);
    }

    public void Stop() {
        attackAnimationTimer.Stop();
        Complete = true;
    }
}

public class MoveStrategy : IActionStrategy
{
    readonly NavMeshAgent agent;
    readonly Func<Vector3> destination;
    private readonly DogSO dog;
    readonly float stoppingDistance;

    public bool CanPerform => !Complete;
    public bool Complete   => agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending;

    public MoveStrategy(NavMeshAgent agent, Func<Vector3> destination, DogSO dog, float stoppingDistance = 2.15f) {
        this.agent = agent;
        this.destination = destination;
        this.dog = dog;
        this.stoppingDistance = stoppingDistance;
    }

    public void Start() {
        dog.StoppingDistance = stoppingDistance;
        agent.SetDestination(destination());
        Debug.LogWarning($"Start MoveStrategy to {destination()}");
    }

    public void Stop() {
        agent.transform.LookAt(destination());
        agent.ResetPath();
    }
}

public class WanderStrategy : IActionStrategy
{
    private readonly NavMeshAgent agent;
    private readonly DogSO dog;
    private readonly CountdownTimer wanderTimer;
    private readonly float wanderRadius;
    private const float stoppingDistance = 2f;

    public bool CanPerform => !Complete;

    public bool Complete => agent.remainingDistance <= stoppingDistance && !agent.pathPending;

    public WanderStrategy(NavMeshAgent agent, DogSO dog, float wanderRadius) {
        this.agent = agent;
        this.dog = dog;
        this.wanderRadius = wanderRadius;

        wanderTimer = new CountdownTimer(5f);
        wanderTimer.OnTimerStart += () => { };
        wanderTimer.OnTimerStop += agent.ResetPath;
    }

    public void Start() {
        dog.StoppingDistance = stoppingDistance;

        Vector3 randomDirection = (UnityEngine.Random.insideUnitSphere * wanderRadius).With(y: 0);
        NavMeshHit hit;

        if (NavMesh.SamplePosition(agent.transform.position + randomDirection, out hit, wanderRadius, 1)) {
            agent.SetDestination(hit.position);
        }
    }

    public void Stop() {
        agent.ResetPath();
    }
}

public class DiggingStrategy : IActionStrategy
{
    private readonly AnimationController animations;
    private readonly CountdownTimer digAnimationTimer;
    private readonly CountdownTimer itemPopUpTimer;
    private readonly Transform rayCastTransform;
    private readonly LayerMask itemsLayer;

    public bool CanPerform => !Complete;

    public bool Complete { get; private set; }

    public DiggingStrategy(AnimationController animations, DogSO dog, Transform rayCastTransform, float funFactor = 100, float aggressionLost = 25f) {
        this.animations = animations;
        this.rayCastTransform = rayCastTransform;
        itemsLayer = LayerMask.GetMask("Items");


        digAnimationTimer = new CountdownTimer(7f);
        itemPopUpTimer = new CountdownTimer(4.5f);
        digAnimationTimer.OnTimerStart += () => { Complete = false; };
        digAnimationTimer.OnTimerStop += () => {
            dog.Fun += funFactor;
            dog.Aggression -= aggressionLost;
            Complete = true;
        };
        itemPopUpTimer.OnTimerStop += () => {
            if (Physics.Raycast(rayCastTransform.position, Vector3.down, out RaycastHit hit, 3f, itemsLayer)) {
                // Prüfe, ob das getroffene Objekt ein IDiggable ist
                if (hit.transform.TryGetComponent(out IDiggable item)) {
                    item.PopUp();
                }
            }
        };
    }

    public void Start() {
        digAnimationTimer.Start();
        itemPopUpTimer.Start();
        animations.StartCoroutine(animations.DogActions(AnimationActionType.Dig));
        animations.SpawnDirtWhileDigging();
    }

    public void Stop() {
        digAnimationTimer.Stop();
        itemPopUpTimer.Stop();
        Complete = true;
    }

    public void OnDrawGizmos() {
        if (rayCastTransform == null) return;

        // SphereCast-Parameter visualisieren
        Vector3 origin = rayCastTransform.position;
        Vector3 direction = Vector3.forward;
        float radius = 1f;
        float distance = 1f;

        // Startpunkt des SphereCasts
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, radius);

        // Richtung des SphereCasts
        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, direction * distance);

        // Endpunkt des SphereCasts
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin + direction * distance, radius);
    }
}


public class IdleStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete   { get; private set; }

    readonly CountdownTimer timer;

    public IdleStrategy(float duration, DogSO dog, float refill = 2) {
        timer = new CountdownTimer(duration);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => {
            dog.Stamina += refill;
            Complete = true;
        };
    }

    public void Start()                 => timer.Start();
    public void Update(float deltaTime) => timer.Tick(deltaTime);
}

public class SleepAndWaitStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete   { get; private set; }

    readonly CountdownTimer sleepAnimationTimer;
    readonly CountdownTimer waitTimer;
    readonly AnimationController animations;

    public SleepAndWaitStrategy(AnimationController animations, DogSO dog, SphereCollider lookAtTrigger, float staminaRefill = 100, float healthRefill = 25f, float aggressionLost = 25f) {
        this.animations = animations;

        sleepAnimationTimer = new CountdownTimer(10f);
        sleepAnimationTimer.OnTimerStart += () => {
            Complete = false;
            dog.RestingSpotAvailable.Value = false;
            lookAtTrigger.radius = 0f;
        };
        sleepAnimationTimer.OnTimerStop += () => {
            dog.Stamina += staminaRefill;
            dog.Health += healthRefill;
            dog.Aggression -= aggressionLost;
            lookAtTrigger.radius = 5f;

            animations.SetAnimatorBool("Sleep_b", false);
        };
        waitTimer = new CountdownTimer(14f);
        waitTimer.OnTimerStart += () => Complete = false;
        waitTimer.OnTimerStop += () => {
            dog.RestingSpotAvailable.Value = true;
            Complete = true;
        };
    }

    public void Start() {
        animations.SetAnimatorBool("Sleep_b", true);
        sleepAnimationTimer.Start();
        waitTimer.Start();
    }

    public void Update(float deltaTime) {
        sleepAnimationTimer.Tick(deltaTime);
        waitTimer.Tick(deltaTime);
    }

    public void Stop() {
        sleepAnimationTimer.Stop();
        waitTimer.Stop();
        Complete = true;
    }
}

public class RestAndWaitStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete   { get; private set; }

    readonly CountdownTimer restAnimationTimer;
    readonly CountdownTimer waitTimer;
    readonly AnimationController animations;

    public RestAndWaitStrategy(AnimationController animations, DogSO dog, float staminaRefill = 50, float healthRefill = 10f, float aggressionLost = 15f) {
        this.animations = animations;

        restAnimationTimer = new CountdownTimer(10f);
        restAnimationTimer.OnTimerStart += () => {
            Complete = false;
            dog.RestingSpotAvailable.Value = false;
        };
        restAnimationTimer.OnTimerStop += () => {
            dog.Stamina += staminaRefill;
            dog.Health += healthRefill;
            dog.Aggression -= aggressionLost;

            animations.SetAnimatorBool("Sit_b", false);
        };
        waitTimer = new CountdownTimer(14f);
        waitTimer.OnTimerStart += () => Complete = false;
        waitTimer.OnTimerStop += () => {
            dog.RestingSpotAvailable.Value = true;
            Complete = true;
        };
    }

    public void Start() {
        animations.SetAnimatorBool("Sit_b", true);
        restAnimationTimer.Start();
        waitTimer.Start();
    }

    public void Update(float deltaTime) {
        restAnimationTimer.Tick(deltaTime);
        waitTimer.Tick(deltaTime);
    }

    public void Stop() {
        restAnimationTimer.Stop();
        waitTimer.Stop();
        Complete = true;
    }
}

public class EatStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete   { get; private set; }

    readonly CountdownTimer eatAnimationTimer;
    readonly CountdownTimer foodDropTimer;
    readonly AnimationController animations;

    readonly float eatAmount = -0.03f;
    
    readonly Transform currentLookAtTarget;

    public EatStrategy(AnimationController animations, Transform food, DogSO dog, SphereCollider lookAtTrigger, Transform lookAtTarget, float saturation = 100f, float aggressionLost = 10f) {
        this.animations = animations;
        
        currentLookAtTarget = lookAtTarget;

        eatAnimationTimer = new CountdownTimer(12.4f);
        eatAnimationTimer.OnTimerStart += () => {
            Complete = false;
            lookAtTarget = food;
            lookAtTrigger.radius = 0f;
        };
        eatAnimationTimer.OnTimerStop += () => {
            dog.Satiety += saturation;
            dog.Aggression -= aggressionLost;
            lookAtTrigger.radius = 5f;
            lookAtTarget = currentLookAtTarget;
            Complete = true;
        };

        foodDropTimer = new CountdownTimer(5f);
        foodDropTimer.OnTimerStart += () => Complete = false;
        foodDropTimer.OnTimerStop += () => { food.position += new Vector3(0f, eatAmount, 0f); };
    }

    public void Start() {
        animations.StartCoroutine(animations.DogActions(AnimationActionType.Eat));
        eatAnimationTimer.Start();
        foodDropTimer.Start();
    }

    public void Update(float deltaTime) {
        eatAnimationTimer.Tick(deltaTime);
        foodDropTimer.Tick(deltaTime);
    }

    public void Stop() {
        eatAnimationTimer.Stop();
        foodDropTimer.Stop();

        Complete = true;
    }
}

public class DrinkStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete   { get; private set; }

    readonly CountdownTimer drinkAnimationTimer;
    readonly CountdownTimer waterDropTimer;
    readonly AnimationController animations;
    readonly float drinkAmount = -0.02f;


    public DrinkStrategy(AnimationController animations, Transform water, DogSO dog, SphereCollider lookAtTrigger, float hydration = 100f) {
        this.animations = animations;

        drinkAnimationTimer = new CountdownTimer(11f);
        drinkAnimationTimer.OnTimerStart += () => {
            Complete = false;
            lookAtTrigger.radius = 0f;
        };
        drinkAnimationTimer.OnTimerStop += () => {
            dog.Hydration += hydration;
            lookAtTrigger.radius = 5f;
            Complete = true;
        };

        waterDropTimer = new CountdownTimer(4f);
        waterDropTimer.OnTimerStop += () => { water.position += new Vector3(0f, drinkAmount, 0f); };
    }

    public void Start() {
        animations.StartCoroutine(animations.DogActions(AnimationActionType.Drink));
        drinkAnimationTimer.Start();
        waterDropTimer.Start();
    }

    public void Update(float deltaTime) {
        drinkAnimationTimer.Tick(deltaTime);
        waterDropTimer.Tick(deltaTime);
    }

    public void Stop() {
        drinkAnimationTimer.Stop();
        waterDropTimer.Stop();
        Complete = true;
    }
}

// public class FetchBallStrategy : IActionStrategy
// {
//     public bool CanPerform => true;
//     public bool Complete   { get; private set; }
//
//     private readonly NavMeshAgent agent;
//     private readonly AnimationController animations;
//     private readonly GameObject ball;
//     private readonly Transform playerTransform;
//     private readonly Transform objectGrabPoint;
//     private readonly ScriptableBoolValue ballThrown;
//     private readonly ScriptableBoolValue ballInHand;
//     private readonly float pickupRange;
//     private readonly float dropRange;
//
//     private enum FetchState
//     {
//         MovingToBall,
//         PickingUpBall,
//         ReturningToPlayer,
//         DroppingBall,
//         Completed
//     }
//
//     private FetchState currentState;
//     private CountdownTimer pickupAnimationTimer;
//     private CountdownTimer dropAnimationTimer;
//
//     public FetchBallStrategy(NavMeshAgent agent, AnimationController animations, GameObject ball,
//         Transform playerTransform,
//         Transform objectGrabPoint, ScriptableBoolValue ballInHand, ScriptableBoolValue ballThrown,
//         float pickupRange = 2f, float dropRange = 2f) {
//         this.agent = agent;
//         this.animations = animations;
//         this.ball = ball;
//         this.playerTransform = playerTransform;
//         this.objectGrabPoint = objectGrabPoint;
//         this.pickupRange = pickupRange;
//         this.dropRange = dropRange;
//         this.ballInHand = ballInHand;
//         this.ballThrown = ballThrown;
//
//         // Timer für die Aufheb-Animation
//         pickupAnimationTimer = new CountdownTimer(2.4f);
//         pickupAnimationTimer.OnTimerStart += () => Complete = false;
//         pickupAnimationTimer.OnTimerStop += () => {
//             animations.Locomotion();
//             if (ball.TryGetComponent(out GrabbableObject grabbableObject)) {
//                 grabbableObject.Grab(objectGrabPoint);
//                 currentState = FetchState.ReturningToPlayer;
//             }
//         };
//
//         // Timer für die Ableg-Animation
//         dropAnimationTimer = new CountdownTimer(2.4f);
//         dropAnimationTimer.OnTimerStart += () => Complete = false;
//         dropAnimationTimer.OnTimerStop += () => {
//             if (ball.TryGetComponent(out GrabbableObject grabbableObject)) {
//                 grabbableObject.Drop();
//                 currentState = FetchState.Completed;
//                 Complete = true;
//             }
//         };
//     }
//
//     public void Start() {
//         Complete = false;
//
//         currentState = FetchState.MovingToBall;
//         animations.SetSpeed(agent.velocity.magnitude);
//         agent.stoppingDistance = pickupRange - 0.2f;
//         agent.SetDestination(ball.transform.position);
//     }
//
//     public void Update(float deltaTime) {
//         switch (currentState) {
//             case FetchState.MovingToBall:
//                 MoveToBall();
//                 break;
//             case FetchState.PickingUpBall:
//                 PickUpBall(deltaTime);
//                 break;
//             case FetchState.ReturningToPlayer:
//                 ReturnToPlayer();
//                 break;
//             case FetchState.DroppingBall:
//                 DropBall(deltaTime);
//                 break;
//             case FetchState.Completed:
//                 // Bereits abgeschlossen
//                 break;
//         }
//     }
//
//     private void MoveToBall() {
//         // Setze Laufanimation
//         animations.Locomotion();
//
//         // Überprüfe, ob Ziel erreicht wurde
//         if (Vector3.Distance(agent.transform.position, ball.transform.position) <= pickupRange) {
//             currentState = FetchState.PickingUpBall;
//         }
//     }
//
//     private void PickUpBall(float deltaTime) {
//         if (!pickupAnimationTimer.IsRunning) {
//             animations.Eat(); // Eat als aufheben
//             pickupAnimationTimer.Start();
//         }
//
//         pickupAnimationTimer.Tick(deltaTime);
//     }
//
//     private void ReturnToPlayer() {
//         agent.stoppingDistance = dropRange - 0.2f;
//         agent.SetDestination(playerTransform.position);
//
//         // // Setze Laufanimation
//         // animations.Locomotion();
//         // animations.SetSpeed(agent.velocity.magnitude);
//
//         // Überprüfe, ob Ziel erreicht wurde
//         if (Vector3.Distance(agent.transform.position, playerTransform.position) <= dropRange) {
//             currentState = FetchState.DroppingBall;
//         }
//     }
//
//     private void DropBall(float deltaTime) {
//         if (!dropAnimationTimer.IsRunning) {
//             animations.Eat();
//             dropAnimationTimer.Start();
//             if (ball.TryGetComponent(out GrabbableObject grabbableObject)) {
//                 grabbableObject.Drop();
//                 // Explizit ballThrown auf false setzen, um zu verhindern, dass die FetchBall-Aktion sofort wieder ausgeführt wird
//                 ballThrown.Value = false;
//             }
//         }
//
//         // Wichtig: Stelle sicher, dass der Ball wirklich abgelegt wird, wenn die Strategie gestoppt wird
//         if (currentState == FetchState.ReturningToPlayer || currentState == FetchState.DroppingBall) {
//             if (ball.TryGetComponent(out GrabbableObject grabbableObject)) {
//                 grabbableObject.Drop();
//                 ballThrown.Value = false;
//                 ballInHand.Value = false;
//             }
//         }
//
//         dropAnimationTimer.Tick(deltaTime);
//     }
//
//     public void Stop() {
//         if (pickupAnimationTimer.IsRunning) {
//             pickupAnimationTimer.Stop();
//         }
//
//         if (dropAnimationTimer.IsRunning) {
//             dropAnimationTimer.Stop();
//         }
//
//         animations.Locomotion();
//         agent.ResetPath();
//         Complete = true;
//     }
// }

public class SeekAttentionStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete   { get; private set; }

    readonly CountdownTimer begAnimationTimer;
    readonly NavMeshAgent navMeshAgent;
    readonly DogSO dog;
    readonly AnimationController animations;
    readonly Transform playerPos;

    public SeekAttentionStrategy(NavMeshAgent navMeshAgent, AnimationController animations, Transform playerPos, DogSO dog, float fun = 100) {
        this.navMeshAgent = navMeshAgent;
        this.animations = animations;
        this.dog = dog;
        this.playerPos = playerPos;


        begAnimationTimer = new CountdownTimer(14f);
        begAnimationTimer.OnTimerStart += () => {
            navMeshAgent.transform.LookAt(playerPos);
            dog.SeekingAttention = true;
            Complete = false;
        };
        begAnimationTimer.OnTimerStop += () => {
            dog.Fun += fun;
            dog.SeekingAttention = false;
            Complete = true;
        };
    }

    public void Start() {
        dog.StoppingDistance = 2.1f;

        animations.StartCoroutine(animations.DogActions(AnimationActionType.Beg));
        begAnimationTimer.Start();
    }

    public void Update(float deltaTime) {
        navMeshAgent.transform.LookAt(playerPos);
        begAnimationTimer.Tick(deltaTime);
    }

    public void Stop() {
        begAnimationTimer.Stop();
        Complete = true;
    }
}

public class WaitForBallThrow : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete   { get; private set; }

    readonly NavMeshAgent agent;
    readonly AnimationController animations;
    readonly CountdownTimer standUpTimer;
    readonly bool ballThrown;

    public WaitForBallThrow(NavMeshAgent agent, DogSO dog, AnimationController animations) {
        if (dog.BallThrown.Value) Complete = true;

        this.agent = agent;
        this.animations = animations;
        ballThrown = dog.BallThrown.Value;

        standUpTimer = new CountdownTimer(2f);
        standUpTimer.OnTimerStart += () => { animations.SetAnimatorBool("Sit_b", false); };
        standUpTimer.OnTimerStop += () => {
            agent.speed = 2.5f;
            Complete = true;
        };
    }

    public void Start() {
        agent.speed = 0f;
        animations.SetAnimatorBool("Sit_b", true);
    }

    public void Update(float deltaTime) {
        standUpTimer.Tick(deltaTime);
        if (ballThrown) standUpTimer.Start();
    }

    public void Stop() {
        standUpTimer.Stop();
    }
}

public class PickUpBallStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete   { get; private set; }

    private readonly NavMeshAgent agent;
    private readonly AnimationController animations;
    private readonly DogSO dog;
    private readonly GameObject ball;
    private readonly Transform objectGrabPoint;
    private readonly float pickupRange;

    private readonly CountdownTimer pickUpAnimationTimer;
    private readonly CountdownTimer pickUpTimer;

    private enum PickUpState
    {
        MovingToBall,
        PickingUpBall,
        Completed
    }

    private PickUpState currentState;

    public PickUpBallStrategy(NavMeshAgent agent, AnimationController animations, GameObject ball, Transform objectGrabPoint, DogSO dog, float pickupRange = 2.15f) {
        this.agent = agent;
        this.animations = animations;
        this.dog = dog;
        this.ball = ball;
        this.objectGrabPoint = objectGrabPoint;
        this.pickupRange = pickupRange;

        pickUpAnimationTimer = new CountdownTimer(12f);
        pickUpTimer = new CountdownTimer(5.7f);

        pickUpAnimationTimer.OnTimerStart += () => {
            animations.StartDogAction(AnimationActionType.Eat);
            Complete = false;
        };
        pickUpAnimationTimer.OnTimerStop += () => {
            dog.ReturnBall.Value = true;
            currentState = PickUpState.Completed;
            Complete = true;
        };

        pickUpTimer.OnTimerStop += () => {
            if (ball.TryGetComponent(out GrabbableObject grabbableObject)) {
                grabbableObject.Grab(objectGrabPoint);
            }

            currentState = PickUpState.Completed;
        };
    }

    public void Start() {
        dog.StoppingDistance = pickupRange;
        Complete = false;
        currentState = PickUpState.MovingToBall;
        agent.SetDestination(ball.transform.position);
    }

    public void Update(float deltaTime) {
        switch (currentState) {
            case PickUpState.MovingToBall:
                MoveToBall();
                break;
            case PickUpState.PickingUpBall:
                PickUpBall(deltaTime);
                break;
            case PickUpState.Completed:
                Complete = true;
                break;
        }
    }

    private void MoveToBall() {
        if (Vector3.Distance(agent.transform.position, ball.transform.position) <= pickupRange && !agent.pathPending) {
            agent.transform.LookAt(ball.transform);
            currentState = PickUpState.PickingUpBall;
            pickUpAnimationTimer.Start();
            pickUpTimer.Start();
        }
        else agent.SetDestination(ball.transform.position);
    }

    private void PickUpBall(float deltaTime) {
        if (!dog.ballAvailable) Complete = true;
        if (Vector3.Distance(agent.transform.position, ball.transform.position) <= pickupRange) {
            pickUpAnimationTimer.Tick(deltaTime);
            pickUpTimer.Tick(deltaTime);
            dog.ballInMouth = true;
            dog.ballAvailable.Value = false;
        }
        else currentState = PickUpState.MovingToBall;
    }

    public void Stop() {
        pickUpAnimationTimer.Stop();
        pickUpTimer.Stop();
        agent.ResetPath();
        Complete = true;
    }
}

public class DropBallStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete   { get; private set; }

    private NavMeshAgent agent;
    private readonly AnimationController animations;
    private readonly DogSO dog;
    private readonly Transform playerTransform;
    private readonly float dropRange;
    private readonly ScriptableBoolValue ballAvailable;

    private readonly CountdownTimer dropAnimationTimer;

    private enum DropState
    {
        MovingToPlayer,
        DroppingBall,
        Completed
    }

    private DropState currentState;

    public DropBallStrategy(NavMeshAgent agent, AnimationController animations, DogSO dog, Transform playerTransform, GameObject ball, Transform objectGrabPoint, float dropRange = 5f) {
        this.agent = agent;
        this.animations = animations;
        this.dog = dog;
        this.playerTransform = playerTransform;
        ballAvailable = dog.ballAvailable;
        
        dropAnimationTimer = new CountdownTimer(10f);
        dropAnimationTimer.OnTimerStart += () => {
            animations.StartDogAction(AnimationActionType.ShakeToy);
            if (ball.TryGetComponent(out ShakeBall ballShake)) {
                ballShake.Shake(objectGrabPoint);
            }

            Complete = false;
        };
        dropAnimationTimer.OnTimerStop += () => {
            if (ball.TryGetComponent(out GrabbableObject grabbableObject)) {
                grabbableObject.Drop();
                dog.ballInMouth = false;
                ballAvailable.Value = true;
            }

            agent.speed = 2.5f;
            dog.Fun += 100f;
            dog.ReturnBall.Value = false;
            dog.BallReturned.Value = true;
            currentState = DropState.Completed;
            Complete = true;
        };
    }

    public void Start() {
        if (!dog.ballAvailable) Complete = true;
        Complete = false;
        dog.StoppingDistance = dropRange;
        currentState = DropState.MovingToPlayer;
        agent.stoppingDistance = dropRange - 0.2f;
        agent.SetDestination(playerTransform.position);
    }

    public void Update(float deltaTime) {
        switch (currentState) {
            case DropState.MovingToPlayer:
                MoveToPlayer();
                break;
            case DropState.DroppingBall:
                DropBall(deltaTime);
                break;
            case DropState.Completed:
                // Bereits abgeschlossen
                break;
        }
    }

    private void MoveToPlayer() {
        if (!dog.ballInMouth) Complete = true;

        if (Vector3.Distance(agent.transform.position, playerTransform.position) <= dropRange && !agent.pathPending) {
            agent.speed = 0f;
            currentState = DropState.DroppingBall;
            dropAnimationTimer.Start();
        }
    }

    private void DropBall(float deltaTime) {
        if (!dog.ballInMouth) Complete = true;
        else dropAnimationTimer.Tick(deltaTime);
    }

    public void Stop() {
        dropAnimationTimer.Stop();
        agent.ResetPath();
        agent.speed = 2.5f;
        dog.ReturnBall.Value = false;
        Complete = true;
    }
}

public class WaitForActionStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete   { get; private set; }

    private readonly NavMeshAgent agent;
    private readonly AnimationController animations;
    private readonly DogSO dog;
    private readonly Transform playerTransform;
    private readonly CountdownTimer standUpAnimationTimer;

    public WaitForActionStrategy(NavMeshAgent agent, AnimationController animations, DogSO dog, Transform playerTransform) {
        this.agent = agent;
        this.animations = animations;
        this.dog = dog;
        this.playerTransform = playerTransform;

        standUpAnimationTimer = new CountdownTimer(4f);
        standUpAnimationTimer.OnTimerStart += () => {
            agent.speed = 0f;
            animations.SetAnimatorBool("Sit_b", false);
        };
        standUpAnimationTimer.OnTimerStop += () => {
            agent.speed = 2.5f;
            dog.DogCalled = false;
            Complete = true;
        };
    }

    public void Start() {
        dog.StoppingDistance = 5.0f;
        agent.transform.LookAt(playerTransform);
        Complete = false;
        animations.SetAnimatorBool("Sit_b", true);
    }

    public void Update(float deltaTime) {
        if (dog.BallThrown || !dog.DogCalled) {
            standUpAnimationTimer.Start();
            Complete = true;
        }
        standUpAnimationTimer.Tick(deltaTime);
    }

    public void Stop() {
        animations.SetAnimatorBool("Sit_b", false);
        standUpAnimationTimer.Stop();
        Complete = true;
    }
}