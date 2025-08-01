using System;
using System.Collections.Generic;
using System.Linq;
using ImprovedTimers;
using ScriptableValues;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = System.Random;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AnimationController))]
[System.Serializable]
public class GoapAgent : MonoBehaviour
{
    #region Class Members

    // [InlineEditor, AssetList(Path = "/_Project/Scriptable Objects/Scriptable Values", AutoPopulate = true)]
    // public List<ScriptableFloatValue> floatValues;

    [FoldoutGroup("Stats", expanded: true), SerializeField, InlineEditor]
    private DogSO dog;
    
    [FoldoutGroup("Known Locations", expanded: false), SerializeField]
    private Blackboard blackboard;
    
    [FoldoutGroup("Transforms"), SerializeField]
    private Transform objectGrabPointPosition;
    
    [FoldoutGroup("Transforms"), SerializeField]
    private Transform diggingRayCastPosition;
    
    [FoldoutGroup("Transforms"), SerializeField]
    private Transform lookAtTarget;
    
    [FoldoutGroup("Colliders"), SerializeField]
    private SphereCollider lookAtTrigger;

    [FoldoutGroup("Health Bar", expanded: false), SerializeField]
    private Slider healthBarSlider;
    
    public HashSet<AgentAction> actions;
    
    public NavMeshAgent navMeshAgent;
    private AnimationController animations;
    private Rigidbody rb;
    private CountdownTimer statsTimer;
    private GameObject target;
    private Vector3 destination;
    private Dictionary<Beliefs, AgentBelief> beliefs;
    private HashSet<AgentGoal> goals;
    private AgentGoal lastGoal;
    private AgentGoal currentGoal;
    private ActionPlan actionPlan;
    private AgentAction currentAction;
    private IGoapPlanner gPlanner;
    private IKHandler ikHandler;
    
    private Obstacle obstacle1;
    private Obstacle obstacle2;
    private Obstacle obstacle3;
    private Obstacle obstacle4;
    
    #endregion

    void Awake() {
        // navMeshAgent = GetComponent<NavMeshAgent>();
        animations = GetComponent<AnimationController>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        ikHandler = GetComponent<IKHandler>();
        obstacle1 = blackboard.Obstacle1.GetComponent<Obstacle>();
        obstacle2 = blackboard.Obstacle2.GetComponent<Obstacle>();
        obstacle3 = blackboard.Obstacle3.GetComponent<Obstacle>();
        obstacle4 = blackboard.Obstacle4.GetComponent<Obstacle>();
        
        gPlanner = new GoapPlanner();
    }

    void Start() {
        SetupTimers();
        SetupBeliefs();
        SetupAndUpdateActions();
        SetupAndUpdateGoals();
        SetDominance(dog.Dominance);
        
        navMeshAgent.stoppingDistance = dog.StoppingDistance;
    }
    
    void Update() {
        statsTimer.Tick(Time.deltaTime);
        animations.SetSpeed(navMeshAgent.velocity.magnitude);
        healthBarSlider.value = dog.Health;

        UpdateActionPlan();
        HandleDeath();
        navMeshAgent.stoppingDistance = dog.StoppingDistance;
    }

    [Button("Update Beliefs"), FoldoutGroup("Buttons"), PropertyOrder(-10)]
    void SetupBeliefs() {
        beliefs = new Dictionary<Beliefs, AgentBelief>();
        BeliefFactory factory = new BeliefFactory(this, beliefs);

        factory.AddBelief(Beliefs.Nothing, () => false);
        factory.AddBelief(Beliefs.Idle, () => !navMeshAgent.hasPath);
        factory.AddBelief(Beliefs.IsMoving, () => navMeshAgent.hasPath);
        factory.AddBelief(Beliefs.DogIsExhausted, () => dog.Stamina < 30);
        factory.AddBelief(Beliefs.DogIsRested, () => dog.Stamina >= 70);
        factory.AddBelief(Beliefs.DogIsHungry, () => dog.Satiety < 30);
        factory.AddBelief(Beliefs.DogIsNotHungry, () => dog.Satiety >= 70);
        factory.AddBelief(Beliefs.DogIsThirsty, () => dog.Hydration < 30);
        factory.AddBelief(Beliefs.DogIsNotThirsty, () => dog.Hydration >= 70);
        factory.AddBelief(Beliefs.DogIsHappy, () => dog.Fun >= 70);
        factory.AddBelief(Beliefs.DogIsBored, () => dog.Fun < 30);
        ;
        factory.AddBelief(Beliefs.BallInHand, () => dog.BallInHand);
        factory.AddBelief(Beliefs.DogAtPlayerWithBall, () => false);
        factory.AddBelief(Beliefs.WaitingForBall, () => false);
        factory.AddBelief(Beliefs.FetchBall, () => dog.FetchBall);
        factory.AddBelief(Beliefs.BallThrown, () => dog.BallThrown);
        factory.AddBelief(Beliefs.ReturnBall, () => dog.ReturnBall);
        factory.AddBelief(Beliefs.BallReturned, () => dog.BallReturned);
        factory.AddBelief(Beliefs.DropBall, () => false);
        factory.AddBelief(Beliefs.DogCalled, () => dog.DogCalled);
        factory.AddBelief(Beliefs.FollowPlayer, () => false);
        factory.AddBelief(Beliefs.FollowCommand, () => false);
        
        factory.AddBelief(Beliefs.FoodBowl1IsAvailable, () => dog.FoodBowl1Available);
        factory.AddBelief(Beliefs.FoodBowl2IsAvailable, () => dog.FoodBowl2Available);
        factory.AddBelief(Beliefs.WaterBowl1IsAvailable, () => dog.WaterBowl1Available.Value);
        factory.AddBelief(Beliefs.WaterBowl2IsAvailable, () => dog.WaterBowl2Available.Value);
        factory.AddBelief(Beliefs.RestingSpotIsAvailable, () => dog.RestingSpotAvailable);

        factory.AddLocationBelief(Beliefs.DogAtRestingPosition, 3f, blackboard.RestingPosition);
        factory.AddLocationBelief(Beliefs.DogAtFoodBowl1, 2.3f, blackboard.FoodBowl1);
        factory.AddLocationBelief(Beliefs.DogAtFoodBowl2, 2.3f, blackboard.FoodBowl2);
        factory.AddLocationBelief(Beliefs.DogAtWaterBowl1, 2.3f, blackboard.WaterBowl1);
        factory.AddLocationBelief(Beliefs.DogAtWaterBowl2, 2.3f, blackboard.WaterBowl2);
        factory.AddLocationBelief(Beliefs.DogAtObstacle1, 4f, blackboard.Obstacle1);
        factory.AddLocationBelief(Beliefs.DogAtObstacle2, 4f, blackboard.Obstacle2);
        factory.AddLocationBelief(Beliefs.DogAtObstacle3, 4f, blackboard.Obstacle3);
        factory.AddLocationBelief(Beliefs.DogAtObstacle4, 4f, blackboard.Obstacle4);
        factory.AddLocationBelief(Beliefs.DogAtBall, 2.2f, blackboard.Ball.transform.position);
        factory.AddLocationBelief(Beliefs.DogAtPlayer, 5f, blackboard.PlayerTransform.position);
        
        factory.AddBelief(Beliefs.AttackingRageVictim, () => false);
        factory.AddBelief(Beliefs.Attacking, () => false);
    }
    
    [Button("Update Actions"), FoldoutGroup("Buttons"), PropertyOrder(-10)]
    protected virtual void SetupAndUpdateActions() {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder(ActionType.Relax)
            .WithStrategy(new IdleStrategy(dog.Settings.restingDuration, dog))
            .AddEffect(beliefs[Beliefs.Nothing])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.WanderAround)
            .WithStrategy(new WanderStrategy(navMeshAgent, dog, dog.Settings.wanderRadius))
            .AddEffect(beliefs[Beliefs.IsMoving])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.MoveToRestArea)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => blackboard.RestingPosition.position, dog, 2.0f))
            .AddEffect(beliefs[Beliefs.DogAtRestingPosition])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.MoveToFoodBowl1)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => blackboard.FoodBowl1.position, dog, dog.Settings.pickUpDistance))
            .AddEffect(beliefs[Beliefs.DogAtFoodBowl1])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.EatAtBowl1)
            .WithCost(dog.EatAtBowl1Costs)
            .WithStrategy(new EatStrategy(animations, blackboard.FoodBowl1, dog, lookAtTrigger, lookAtTarget))
            .AddPrecondition(beliefs[Beliefs.FoodBowl1IsAvailable])
            .AddPrecondition(beliefs[Beliefs.DogAtFoodBowl1])
            .AddPrecondition(beliefs[Beliefs.DogIsHungry])
            .AddEffect(beliefs[Beliefs.DogIsNotHungry])
            .Build());
        
        actions.Add(new AgentAction.Builder(ActionType.MoveToFoodBowl2)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => blackboard.FoodBowl2.position, dog, dog.Settings.pickUpDistance))
            .AddEffect(beliefs[Beliefs.DogAtFoodBowl2])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.EatAtBowl2)
            .WithCost(dog.EatAtBowl2Costs)
            .WithStrategy(new EatStrategy(animations, blackboard.FoodBowl2, dog, lookAtTrigger, lookAtTarget))
            .AddPrecondition(beliefs[Beliefs.FoodBowl2IsAvailable])
            .AddPrecondition(beliefs[Beliefs.DogAtFoodBowl2])
            .AddPrecondition(beliefs[Beliefs.DogIsHungry])
            .AddEffect(beliefs[Beliefs.DogIsNotHungry])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.MoveToWaterBowl1)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => blackboard.WaterBowl1.position, dog, dog.Settings.pickUpDistance))
            .AddEffect(beliefs[Beliefs.DogAtWaterBowl1])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.DrinkAtBowl1)
            .WithCost(dog.DrinkAtBowl1Costs)
            .WithStrategy(new DrinkStrategy(animations, blackboard.WaterBowl1, dog, lookAtTrigger))
            .AddPrecondition(beliefs[Beliefs.WaterBowl1IsAvailable])
            .AddPrecondition(beliefs[Beliefs.DogAtWaterBowl1])
            .AddPrecondition(beliefs[Beliefs.DogIsThirsty])
            .AddEffect(beliefs[Beliefs.DogIsNotThirsty])
            .Build());
        
        actions.Add(new AgentAction.Builder(ActionType.MoveToWaterBowl2)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => blackboard.WaterBowl2.position, dog, dog.Settings.pickUpDistance))
            .AddEffect(beliefs[Beliefs.DogAtWaterBowl2])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.DrinkAtBowl2)
            .WithCost(dog.DrinkAtBowl2Costs)
            .WithStrategy(new DrinkStrategy(animations, blackboard.WaterBowl2, dog, lookAtTrigger))
            .AddPrecondition(beliefs[Beliefs.WaterBowl2IsAvailable])
            .AddPrecondition(beliefs[Beliefs.DogAtWaterBowl2])
            .AddPrecondition(beliefs[Beliefs.DogIsThirsty])
            .AddEffect(beliefs[Beliefs.DogIsNotThirsty])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.Sleep)
            .WithCost(dog.SleepCosts)
            .WithStrategy(new SleepAndWaitStrategy(animations, dog, lookAtTrigger))
            .AddPrecondition(beliefs[Beliefs.RestingSpotIsAvailable])
            .AddPrecondition(beliefs[Beliefs.DogAtRestingPosition])
            .AddPrecondition(beliefs[Beliefs.DogIsExhausted])
            .AddEffect(beliefs[Beliefs.DogIsRested])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.Rest)
            .WithCost(dog.RestCosts)
            .WithStrategy(new RestAndWaitStrategy(animations, dog))
            .AddPrecondition(beliefs[Beliefs.RestingSpotIsAvailable])
            .AddPrecondition(beliefs[Beliefs.DogAtRestingPosition])
            .AddPrecondition(beliefs[Beliefs.DogIsExhausted])
            .AddEffect(beliefs[Beliefs.DogIsRested])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.MoveToPlayer)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => blackboard.PlayerTransform.position,  dog,4f))
            .AddEffect(beliefs[Beliefs.DogAtPlayer])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.SeekAttention)
            .WithCost(dog.SeekAttentionCosts)
            .WithStrategy(new SeekAttentionStrategy(navMeshAgent, animations, blackboard.PlayerTransform, dog))
            .AddPrecondition(beliefs[Beliefs.DogIsBored])
            .AddPrecondition(beliefs[Beliefs.DogAtPlayer])
            .AddEffect(beliefs[Beliefs.DogIsHappy])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.MoveToObstacle1)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => blackboard.Obstacle1.position,  dog,1.9f))
            .AddEffect(beliefs[Beliefs.DogAtObstacle1])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.AttackObstacle1)
            .WithCost(dog.RageObstacle1Costs + obstacle1.actionCostIncrease)
            .WithStrategy(new AttackStrategy(navMeshAgent, animations, dog, obstacle1))
            .AddPrecondition(beliefs[Beliefs.DogAtObstacle1])
            .AddPrecondition(beliefs[Beliefs.DogIsBored])
            .AddEffect(beliefs[Beliefs.DogIsHappy])
            .Build());
        
        actions.Add(new AgentAction.Builder(ActionType.MoveToObstacle2)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => blackboard.Obstacle2.position,  dog,1.7f))
            .AddEffect(beliefs[Beliefs.DogAtObstacle2])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.AttackObstacle2)
            .WithCost(dog.RageObstacle2Costs + obstacle2.actionCostIncrease)
            .WithStrategy(new AttackStrategy(navMeshAgent, animations, dog, obstacle2))
            .AddPrecondition(beliefs[Beliefs.DogAtObstacle2])
            .AddPrecondition(beliefs[Beliefs.DogIsBored])
            .AddEffect(beliefs[Beliefs.DogIsHappy])
            .Build());
        
        actions.Add(new AgentAction.Builder(ActionType.MoveToObstacle3)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => blackboard.Obstacle3.position, dog, 1.8f))
            .AddEffect(beliefs[Beliefs.DogAtObstacle3])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.AttackObstacle3)
            .WithCost(dog.RageObstacle3Costs + obstacle3.actionCostIncrease)
            .WithStrategy(new AttackStrategy(navMeshAgent, animations, dog, obstacle3))
            .AddPrecondition(beliefs[Beliefs.DogAtObstacle3])
            .AddPrecondition(beliefs[Beliefs.DogIsBored])
            .AddEffect(beliefs[Beliefs.DogIsHappy])
            .Build());
        
        actions.Add(new AgentAction.Builder(ActionType.MoveToObstacle4)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => blackboard.Obstacle4.position, dog, 1.9f))
            .AddEffect(beliefs[Beliefs.DogAtObstacle4])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.AttackObstacle4)
            .WithCost(dog.RageObstacle4Costs + obstacle4.actionCostIncrease)
            .WithStrategy(new AttackStrategy(navMeshAgent, animations, dog, obstacle4))
            .AddPrecondition(beliefs[Beliefs.DogAtObstacle4])
            .AddPrecondition(beliefs[Beliefs.DogIsBored])
            .AddEffect(beliefs[Beliefs.DogIsHappy])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.Digging)
            .WithCost(dog.DiggingCosts)
            .WithStrategy(new DiggingStrategy(animations, dog, diggingRayCastPosition))
            .AddPrecondition(beliefs[Beliefs.DogIsBored])
            .AddEffect(beliefs[Beliefs.DogIsHappy])
            .Build());

        // actions.Add(new AgentAction.Builder(ActionType.ComeToPlayer)
        //     .WithStrategy(new MoveStrategy(navMeshAgent, () => knownLocations.PlayerTransform.position, 6.0f))
        //     .AddPrecondition(beliefs[Beliefs.DogCalled])
        //     .AddEffect(beliefs[Beliefs.FollowPlayer])
        //     .Build());

        actions.Add(new AgentAction.Builder(ActionType.WaitForAction)
            .WithStrategy(new WaitForActionStrategy(navMeshAgent, animations, dog, blackboard.PlayerTransform))
            // .AddPrecondition(beliefs[Beliefs.FollowPlayer])
            .AddPrecondition(beliefs[Beliefs.DogCalled])
            .AddEffect(beliefs[Beliefs.FollowCommand])
            .Build());
        
        actions.Add(new AgentAction.Builder(ActionType.PickUpBall)
            .WithStrategy(new PickUpBallStrategy(navMeshAgent, animations, blackboard.Ball, objectGrabPointPosition, dog))
            .AddPrecondition(beliefs[Beliefs.BallThrown])
            .AddEffect(beliefs[Beliefs.ReturnBall])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.DropBall)
            .WithStrategy(new DropBallStrategy(navMeshAgent, animations, dog, blackboard.PlayerTransform, blackboard.Ball, objectGrabPointPosition))
            .AddPrecondition(beliefs[Beliefs.ReturnBall])
            .AddEffect(beliefs[Beliefs.BallReturned])
            .Build());
    }
    
    [Button("Update Goals"), FoldoutGroup("Buttons"), PropertyOrder(-10)]
    void SetupAndUpdateGoals() {
        goals = new HashSet<AgentGoal>();

        goals.Add(new AgentGoal.Builder(GoalType.Idle)
            .WithPriority(dog.IdlePrio)
            .WithDesiredEffect(beliefs[Beliefs.Nothing])
            .Build());

        goals.Add(new AgentGoal.Builder(GoalType.Wander)
            .WithPriority(dog.WanderPrio)
            .WithDesiredEffect(beliefs[Beliefs.IsMoving])
            .Build());

        goals.Add(new AgentGoal.Builder(GoalType.KeepThirstLow)
            .WithPriority(dog.KeepHydrationLevelUpPrio)
            .WithDesiredEffect(beliefs[Beliefs.DogIsNotThirsty])
            .Build());

        goals.Add(new AgentGoal.Builder(GoalType.KeepHungerLow)
            .WithPriority(dog.KeepSatietyLevelUpPrio)
            .WithDesiredEffect(beliefs[Beliefs.DogIsNotHungry])
            .Build());

        goals.Add(new AgentGoal.Builder(GoalType.KeepExhaustionLow)
            .WithPriority(dog.KeepStaminaUpPrio)
            .WithDesiredEffect(beliefs[Beliefs.DogIsRested])
            .Build());

        goals.Add(new AgentGoal.Builder(GoalType.KeepBoredomLow)
            .WithPriority(dog.KeepFunUpPrio)
            .WithDesiredEffect(beliefs[Beliefs.DogIsHappy])
            .Build());

        goals.Add(new AgentGoal.Builder(GoalType.FetchBallAndReturnIt)
            .WithPriority(dog.FetchBallAndReturnItPrio)
            .WithDesiredEffect(beliefs[Beliefs.BallReturned])
            .Build());

        goals.Add(new AgentGoal.Builder(GoalType.FollowCommand)
            .WithPriority(200)
            .WithDesiredEffect(beliefs[Beliefs.FollowCommand])
            .Build());
        
        if (dog.Satiety < 10 || dog.Hydration < 10) {
            goals.Add(new AgentGoal.Builder(GoalType.StayAlive)
                .WithPriority(dog.StayAlive)
                .WithDesiredEffect(beliefs[Beliefs.DogIsNotThirsty])
                .WithDesiredEffect(beliefs[Beliefs.DogIsNotHungry])
                .Build());
        }
    }

    void SetupTimers() {
        statsTimer = new CountdownTimer(1f);
        statsTimer.OnTimerStop += () => {
            UpdateStats();
            statsTimer.Start();
        };
        statsTimer.Start();
    }
    
    void UpdateStats() {
        dog.UpdateDogBehaviour();
        SetupAndUpdateActions();
        SetupAndUpdateGoals();
    }

    void SetDominance(int dominance) {
        navMeshAgent.avoidancePriority = dominance;
    }

    void HandleDeath() {
        if (dog.Health <= 0) {
            animations.SetAnimatorBool("Death_b", true);
            navMeshAgent.speed = 0f;
        }
        else {
            animations.SetAnimatorBool("Death_b", false);
            navMeshAgent.speed = 2.5f;
        }
    }
    
    void HandleTargetChanged() {
        Debug.Log("Target changed, clearing current action and goal");
        // Force the planner to re-evaluate the plan
        currentAction = null;
        currentGoal = null;
    }

    private void UpdateActionPlan() {
        // Update the plan and current action if there is one
        if (currentAction == null) {
            Debug.Log("Calculating any potential new plan");
            CalculatePlan();

            if (actionPlan != null && actionPlan.Actions.Count > 0) {
                navMeshAgent.ResetPath();

                currentGoal = actionPlan.AgentGoal;
                Debug.Log($"Goal: {currentGoal.Type} with {actionPlan.Actions.Count} actions in plan");
                currentAction = actionPlan.Actions.Pop();
                Debug.Log($"Popped action: {currentAction.Type}");
                // Verify all precondition effects are true
                if (currentAction.Preconditions.All(b => b.Evaluate())) {
                    currentAction.Start();
                }
                else {
                    Debug.Log("Preconditions not met, clearing current action and goal");
                    currentAction = null;
                    currentGoal = null;
                }
            }
        }

        // If we have a current action, execute it
        if (actionPlan != null && currentAction != null) {
            currentAction.Update(Time.deltaTime);

            if (currentAction.Complete) {
                Debug.Log($"{currentAction.Type} complete");
                currentAction.Stop();
                currentAction = null;

                if (actionPlan.Actions.Count == 0) {
                    Debug.Log("Plan complete");
                    lastGoal = currentGoal;
                    currentGoal = null;
                }
            }
        }
    }

    void CalculatePlan() {
        var priorityLevel = currentGoal?.Priority ?? 0;

        HashSet<AgentGoal> goalsToCheck = goals;

        // If we have a current goal, we only want to check goals with higher priority
        if (currentGoal != null) {
            Debug.Log("Current goal exists, checking goals with higher priority");
            goalsToCheck = new HashSet<AgentGoal>(goals.Where(g => g.Priority > priorityLevel));
        }

        var potentialPlan = gPlanner.Plan(this, goalsToCheck, lastGoal);
        if (potentialPlan != null) {
            actionPlan = potentialPlan;
        }
    }

// #if UNITY_EDITOR
//     private void OnGUI() {
//         // Nur im Play-Mode anzeigen
//         if (!Application.isPlaying) return;
//
//         // GUI-Stil für bessere Lesbarkeit
//         GUIStyle style = new GUIStyle(GUI.skin.box);
//         style.fontSize = 16;
//         style.normal.textColor = Color.white;
//         style.alignment = TextAnchor.MiddleLeft;
//         style.padding = new RectOffset(10, 10, 5, 5);
//
//         // Hintergrund für die Box
//         Texture2D texture = new Texture2D(1, 1);
//         texture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
//         texture.Apply();
//         style.normal.background = texture;
//
//         // Position und Größe der Box
//         int width = 200;
//         int height = 220;
//         int padding = 10;
//         Rect rect = new Rect(Screen.width - width - padding, padding, width, height);
//
//         // Box mit den Werten zeichnen
//         GUI.Box(rect, "", style);
//
//         GUILayout.BeginArea(rect);
//         GUILayout.Label($"<b>Dog Stats:</b>", style);
//         GUILayout.Label($"Health:    {dog.Health:F1}", style);
//         GUILayout.Label($"Aggression:{dog.Aggression:F1}", style);
//         GUILayout.Label($"Stamina:   {dog.Stamina:F1}", style);
//         GUILayout.Label($"Satiety:   {dog.Satiety:F1}", style);
//         GUILayout.Label($"Hydration: {dog.Hydration:F1}", style);
//         GUILayout.Label($"Fun:       {dog.Fun:F1}", style);
//         GUILayout.EndArea();
//     }
// #endif
}