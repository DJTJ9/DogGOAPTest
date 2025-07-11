using System;
using System.Collections.Generic;
using System.Linq;
using ImprovedTimers;
using ScriptableValues;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
// using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AnimationController))]
[System.Serializable]
public class GoapAgent : MonoBehaviour
{
    #region Class Members

    // [InlineEditor, AssetList(Path = "/_Project/Scriptable Objects/Scriptable Values", AutoPopulate = true)]
    // public List<ScriptableFloatValue> floatValues;

    [FoldoutGroup("Stats", expanded: true), SerializeField, InlineEditor]
    private ScriptableFloatValue stamina;

    [FoldoutGroup("Stats"), SerializeField, InlineEditor]
    private ScriptableFloatValue thirst;

    [FoldoutGroup("Stats"), SerializeField, InlineEditor]
    private ScriptableFloatValue hunger;

    [FoldoutGroup("Stats"), SerializeField, InlineEditor]
    private ScriptableFloatValue boredom;

    [FoldoutGroup("Stats"), SerializeField, InlineEditor]
    private ScriptableBoolValue ballInHand;

    [FoldoutGroup("Stats"), SerializeField, InlineEditor]
    private ScriptableBoolValue ballThrown;

    [FoldoutGroup("Stats"), SerializeField, InlineEditor]
    private ScriptableBoolValue ballReturned;

    [FoldoutGroup("Stats"), SerializeField, InlineEditor]
    private ScriptableBoolValue returnBall;

    [FoldoutGroup("Dog Parameters", expanded: true), InlineEditor]
    [SerializeField]
    private DogParamsSO dogParams;

    [FoldoutGroup("Dog Parameters"), InlineEditor, SerializeField]
    private ActionCostsSO actionCosts;

    [FoldoutGroup("Dog Parameters"), InlineEditor, SerializeField]
    private GoalPrioritiesSO goalPriorities;
    
    [FoldoutGroup("Dog Parameters"), InlineEditor, SerializeField]
    private StatManager statManager;

    [FoldoutGroup("Sensors", expanded: false), SerializeField]
    private Sensor playerSensor;

    [FoldoutGroup("Sensors"), SerializeField]
    private Sensor attackSensor;

    [FoldoutGroup("Known Locations", expanded: false), SerializeField]
    private Transform restingPosition;

    [FoldoutGroup("Known Locations"), SerializeField]
    private Transform foodBowl;

    [FoldoutGroup("Known Locations"), SerializeField]
    private Transform waterBowl;

    [FoldoutGroup("Known Locations"), SerializeField]
    private Transform rageVictim;

    [FoldoutGroup("Known Locations"), SerializeField]
    private Transform doorOnePosition;

    [FoldoutGroup("Known Locations"), SerializeField]
    private Transform doorTwoPosition;

    [FoldoutGroup("Known Locations"), SerializeField]
    private Transform playerTransform;

    [FoldoutGroup("Known Locations"), SerializeField]
    private Transform objectGrabPointPosition;

    public GameObject ball;

    private NavMeshAgent navMeshAgent;
    private AnimationController animations;
    private Rigidbody rb;
    private CountdownTimer statsTimer;
    private GameObject target;
    private Vector3 destination;

    public HashSet<AgentAction> actions;
    private Dictionary<Beliefs, AgentBelief> beliefs;
    private HashSet<AgentGoal> goals;
    private AgentGoal lastGoal;
    private AgentGoal currentGoal;
    private ActionPlan actionPlan;
    private AgentAction currentAction;
    private IGoapPlanner gPlanner;

    #endregion

    void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animations = GetComponent<AnimationController>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        gPlanner = new GoapPlanner();
    }

    void Start() {
        SetupTimers();
        SetupBeliefs(); //beliefs = dogBeliefs.SetupBeliefs();
        SetupActions();
        SetupGoals();
    }

    void OnEnable() => playerSensor.OnTargetChanged += HandleTargetChanged;

    void OnDisable() => playerSensor.OnTargetChanged -= HandleTargetChanged;

    void SetupBeliefs() {
        beliefs = new Dictionary<Beliefs, AgentBelief>();
        BeliefFactory factory = new BeliefFactory(this, beliefs);

        factory.AddBelief(Beliefs.Nothing, () => false);
        factory.AddBelief(Beliefs.Idle, () => !navMeshAgent.hasPath);
        factory.AddBelief(Beliefs.IsMoving, () => navMeshAgent.hasPath);
        factory.AddBelief(Beliefs.DogStaminaLow, () => stamina.Value < 10);
        factory.AddBelief(Beliefs.DogIsRested, () => stamina.Value >= 30);
        factory.AddBelief(Beliefs.DogHungerLow, () => hunger.Value < 10);
        factory.AddBelief(Beliefs.DogIsFed, () => hunger.Value >= 30);
        factory.AddBelief(Beliefs.DogIsThirsty, () => thirst.Value < 10);
        factory.AddBelief(Beliefs.DogIsNotThirsty, () => thirst.Value >= 30);
        factory.AddBelief(Beliefs.DogIsHappy, () => boredom.Value < 90);
        factory.AddBelief(Beliefs.DogIsBored, () => boredom.Value >= 50);
        factory.AddBelief(Beliefs.BallInHand, () => ballInHand.Value);
        ;
        factory.AddBelief(Beliefs.BallThrown, () => ballThrown.Value);
        factory.AddBelief(Beliefs.ReturnBall, () => returnBall.Value);
        factory.AddBelief(Beliefs.BallReturned, () => ballReturned.Value);
        factory.AddBelief(Beliefs.FetchBall, () => !ballThrown.Value && !ballInHand.Value);
        factory.AddBelief(Beliefs.DropBall, () => false);

        factory.AddLocationBelief(Beliefs.DogAtRestingPosition, 3f, restingPosition);
        factory.AddLocationBelief(Beliefs.DogAtFoodBowl, 3f, foodBowl);
        factory.AddLocationBelief(Beliefs.DogAtWaterBowl, 3f, waterBowl);
        factory.AddLocationBelief(Beliefs.DogAtRageVictim, 3f, rageVictim);
        factory.AddLocationBelief(Beliefs.DogAtBall, 2f, ball.transform.position);
        factory.AddLocationBelief(Beliefs.DogAtPlayer, 5f, playerTransform.position);

        // factory.AddSensorBelief(Beliefs.PlayerInRange, playerSensor);
        // factory.AddSensorBelief(Beliefs.PlayerInAttackRange, attackSensor);
        factory.AddBelief(Beliefs.Attacking, () => false); // Player can always be attacked, this will never become true
        factory.AddBelief(Beliefs.AttackingRageVictim, () => false);
    }

    [PropertyOrder(-10)]
    [Button("Update Actions"), FoldoutGroup("Buttons")]
    protected virtual void SetupActions() {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder(ActionType.Relax)
            .WithStrategy(new IdleStrategy(dogParams.restingDuration, stamina))
            .AddEffect(beliefs[Beliefs.Nothing])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.WanderAround)
            .WithStrategy(new WanderStrategy(navMeshAgent, dogParams.wanderRadius))
            .AddEffect(beliefs[Beliefs.IsMoving])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.MoveToRestArea)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position, 2.0f))
            .AddEffect(beliefs[Beliefs.DogAtRestingPosition])
            .Build());

        // // Neue FetchBall-Aktion
        // actions.Add(new AgentAction.Builder(ActionType.FetchBall)
        //     .WithStrategy(new FetchBallStrategy(navMeshAgent, animations, ball, playerPosition, objectGrabPointPosition, ballInHand, ballThrown, 1.5f, 2f))
        //     .AddPrecondition(beliefs[Beliefs.BallThrown]) // Neuen Belief hinzufügen
        //     .AddEffect(beliefs[Beliefs.BallRetrieved])    // Neuen Belief hinzufügen
        //     .Build());

        // actions.Add(new AgentAction.Builder(ActionType.MoveToPlayer)
        //     .WithStrategy(new MoveStrategy(navMeshAgent, animations, () => playerPosition.position, 3f))
        //     .AddPrecondition(beliefs[Beliefs.BallInHand])
        //     .AddEffect(beliefs[Beliefs.FetchBall])
        //     .Build());
        //
        // actions.Add(new AgentAction.Builder(ActionType.MoveToBall)
        //     .WithStrategy(new MoveStrategy(navMeshAgent, animations, () => ball.transform.position))
        //     .AddPrecondition(beliefs[Beliefs.FetchBall])
        //     .AddEffect(beliefs[Beliefs.BallThrown])
        //     .Build());
        //
        // actions.Add(new AgentAction.Builder(ActionType.PickUpBall)
        //     .WithStrategy(new PickUpBallStrategy(navMeshAgent, animations, ball, objectGrabPointPosition, returnBall))
        //     .AddPrecondition(beliefs[Beliefs.BallThrown])
        //     .AddEffect(beliefs[Beliefs.ReturnBall])
        //     .Build());
        //
        // actions.Add(new AgentAction.Builder(ActionType.ReturnToPlayer)
        //     .WithStrategy(new MoveStrategy(navMeshAgent, animations, () => playerPosition.position))
        //     .AddPrecondition(beliefs[Beliefs.ReturnBall])
        //     .AddEffect(beliefs[Beliefs.DropBall])
        //     .Build());
        //
        // actions.Add(new AgentAction.Builder(ActionType.DropBall)
        //     .WithStrategy(new DropBallStrategy(navMeshAgent, animations, ball, objectGrabPointPosition, ballReturned, returnBall))
        //     .AddPrecondition(beliefs[Beliefs.DropBall])
        //     .AddEffect(beliefs[Beliefs.BallReturned])
        //     .Build());

        // actions.Add(new AgentAction.Builder(ActionType.MoveFromWanderToRestArea)
        //     .WithStrategy(new MoveStrategy(navMeshAgent, animations, () => restingPosition.position, 2.0f))
        //     .AddPrecondition(beliefs[Beliefs.DogMoving])
        //     .AddEffect(beliefs[Beliefs.DogAtRestingPosition])
        //     .Build());

        actions.Add(new AgentAction.Builder(ActionType.MoveToEatingPosition)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => foodBowl.position, dogParams.pickUpDistance))
            .AddEffect(beliefs[Beliefs.DogAtFoodBowl])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.Eat)
            .WithStrategy(
                new EatAndWaitStrategy(animations, foodBowl, hunger))
            .AddPrecondition(beliefs[Beliefs.DogAtFoodBowl])
            .AddEffect(beliefs[Beliefs.DogIsFed])
            .Build());

        // Brauch es die spezifischen MoveTos wirklich?
        // actions.Add(new AgentAction.Builder(ActionType.MoveFromFoodBowlToRestArea)
        //     .WithStrategy(new MoveStrategy(navMeshAgent, animations, () => restingPosition.position, 2.0f))
        //     .AddPrecondition(beliefs[Beliefs.DogAtFoodBowl])
        //     .AddEffect(beliefs[Beliefs.DogAtRestingPosition])
        //     .Build());

        actions.Add(new AgentAction.Builder(ActionType.MoveToDrinkingPosition)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => waterBowl.position, dogParams.pickUpDistance))
            .AddEffect(beliefs[Beliefs.DogAtWaterBowl])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.Drink)
            .WithStrategy(new DrinkAndWaitStrategy(animations, waterBowl, thirst))
            .AddPrecondition(beliefs[Beliefs.DogAtWaterBowl])
            .AddEffect(beliefs[Beliefs.DogIsNotThirsty])
            .Build());

        // actions.Add(new AgentAction.Builder(ActionType.MoveFromWaterBowlToRestArea)
        //     .WithStrategy(new MoveStrategy(navMeshAgent, animations, () => restingPosition.position, 2.0f))
        //     .AddPrecondition(beliefs[Beliefs.DogAtWaterBowl])
        //     .AddEffect(beliefs[Beliefs.DogAtRestingPosition])
        //     .Build());

        actions.Add(new AgentAction.Builder(ActionType.Sleep)
            .WithCost(actionCosts.Sleep)
            .WithStrategy(new SleepAndWaitStrategy(animations, stamina))
            .AddPrecondition(beliefs[Beliefs.DogAtRestingPosition])
            .AddEffect(beliefs[Beliefs.DogIsRested])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.Rest)
            .WithCost(actionCosts.Rest)
            .WithStrategy(new IdleStrategy(dogParams.restingDuration, stamina))
            .AddPrecondition(beliefs[Beliefs.DogAtRestingPosition])
            .AddEffect(beliefs[Beliefs.DogIsRested])
            .Build());

        // actions.Add(new AgentAction.Builder(ActionType.ChasePlayer)
        //     .WithStrategy(new MoveStrategy(navMeshAgent, animations, () => beliefs[Beliefs.PlayerInChaseRange].Location, 1.5f))
        //     .AddPrecondition(beliefs[Beliefs.PlayerInChaseRange])
        //     .AddEffect(beliefs[Beliefs.PlayerInChaseRange])
        //     .Build());

        actions.Add(new AgentAction.Builder(ActionType.MoveToPlayer)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => playerTransform.position, 4f))
            .AddEffect(beliefs[Beliefs.DogAtPlayer])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.SeekAttention)
            .WithCost(actionCosts.Attention)
            .WithStrategy(new SeekAttentionStrategy(navMeshAgent, animations, playerTransform, boredom))
            .AddPrecondition(beliefs[Beliefs.DogIsBored])
            .AddPrecondition(beliefs[Beliefs.DogAtPlayer])
            .AddEffect(beliefs[Beliefs.DogIsHappy])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.MoveToRageVictim)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => rageVictim.position, 3.2f))
            // .AddPrecondition(beliefs[Beliefs.DogIdle])
            .AddEffect(beliefs[Beliefs.DogAtRageVictim])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.AttackRageVictim)
            .WithCost(actionCosts.Rage)
            .WithStrategy(new AttackStrategy(animations, boredom))
            .AddPrecondition(beliefs[Beliefs.DogAtRageVictim])
            .AddEffect(beliefs[Beliefs.DogIsHappy])
            .Build());
    }
    
    [PropertyOrder(-10)]
    [Button("Update Goals"), FoldoutGroup("Buttons")]
    void SetupGoals() {
        goals = new HashSet<AgentGoal>();

        goals.Add(new AgentGoal.Builder(GoalType.Idle)
            .WithPriority(goalPriorities.Idle)
            .WithDesiredEffect(beliefs[Beliefs.Nothing])
            .Build());

        goals.Add(new AgentGoal.Builder(GoalType.Wander)
            .WithPriority(goalPriorities.Wander)
            .WithDesiredEffect(beliefs[Beliefs.IsMoving])
            .Build());

        goals.Add(new AgentGoal.Builder(GoalType.KeepThirstLow)
            .WithPriority(goalPriorities.KeepThirstLevelUp)
            .WithDesiredEffect(beliefs[Beliefs.DogIsNotThirsty])
            .Build());

        goals.Add(new AgentGoal.Builder(GoalType.KeepHungerLow)
            .WithPriority(goalPriorities.KeepHungerLevelUp)
            .WithDesiredEffect(beliefs[Beliefs.DogIsFed])
            .Build());

        goals.Add(new AgentGoal.Builder(GoalType.KeepExhaustionLow)
            .WithPriority(goalPriorities.KeepStaminaUp)
            .WithDesiredEffect(beliefs[Beliefs.DogIsRested])
            .Build());

        // goals.Add(new AgentGoal.Builder(GoalType.Attack)
        //     .WithPriority(3)
        //     .WithDesiredEffect(beliefs[Beliefs.Attacking])
        //     .Build());

        goals.Add(new AgentGoal.Builder(GoalType.KeepBoredomLow)
            .WithPriority(goalPriorities.KeepBoredomLow)
            .WithDesiredEffect(beliefs[Beliefs.DogIsHappy])
            .Build());

        goals.Add(new AgentGoal.Builder(GoalType.FetchBallAndReturnIt)
            .WithPriority(goalPriorities.FetchBallAndReturnIt)
            .WithDesiredEffect(beliefs[Beliefs.BallReturned])
            .Build());

        // if (thirst.Value < 10 || hunger.Value < 10) {
        //     goals.Add(new AgentGoal.Builder(GoalType.StayAlive)
        //         .WithPriority(goalPriorities.StayAlive)
        //         .WithDesiredEffect(beliefs[Beliefs.DogIsNotThirsty])
        //         .WithDesiredEffect(beliefs[Beliefs.DogIsFed])
        //         .Build());
        // }
    }

    void SetupTimers() {
        statsTimer = new CountdownTimer(1f);
        statsTimer.OnTimerStop += () => {
            UpdateStats();
            statsTimer.Start();
        };
        statsTimer.Start();
    }

    // TODO eigenes Stat System
    void UpdateStats() {
        // actionCosts.Sleep = Mathf.Clamp(UnityEngine.Random.Range(actionCosts.minSleepCosts, actionCosts.maxSleepCosts), actionCosts.minSleepCosts, actionCosts.maxSleepCosts);
        // actionCosts.Rest = Mathf.Clamp(UnityEngine.Random.Range(actionCosts.minRestCosts, actionCosts.maxRestCosts), actionCosts.minRestCosts, actionCosts.maxRestCosts);
        // actionCosts.Attention = Mathf.Clamp(UnityEngine.Random.Range(actionCosts.minAttentionCosts, actionCosts.maxAttentionCosts), actionCosts.minAttentionCosts, actionCosts.maxAttentionCosts);
        // actionCosts.Rage = Mathf.Clamp(UnityEngine.Random.Range(actionCosts.minRageCosts, actionCosts.maxRageCosts), actionCosts.minRageCosts, actionCosts.maxRageCosts);

        // SetupActions();
        //
        // SetupGoals();
        hunger.Subtract(statManager.HungerLost);
        boredom.Add(statManager.BoredomLost);
        stamina.Subtract(statManager.StaminaLost);
        thirst.Subtract(statManager.ThirstLost);
    }

    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(transform.position, pos) < range;


    // Zugriffsmethoden für Locations
    public Transform GetRestingPosition() => restingPosition;
    public Transform GetFoodBowl()        => foodBowl;
    public Transform GetWaterBowl()       => waterBowl;
    public Transform GetDoorOnePosition() => doorOnePosition;
    public Transform GetDoorTwoPosition() => doorTwoPosition;

    void HandleTargetChanged() {
        Debug.Log("Target changed, clearing current action and goal");
        // Force the planner to re-evaluate the plan
        currentAction = null;
        currentGoal = null;
    }

    void Update() {
        statsTimer.Tick(Time.deltaTime);
        animations.SetSpeed(navMeshAgent.velocity.magnitude);

        UpdateActionPlan();
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

#if UNITY_EDITOR
    private void OnGUI() {
        // Nur im Play-Mode anzeigen
        if (!Application.isPlaying) return;

        // GUI-Stil für bessere Lesbarkeit
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleLeft;
        style.padding = new RectOffset(10, 10, 5, 5);

        // Hintergrund für die Box
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
        texture.Apply();
        style.normal.background = texture;

        // Position und Größe der Box
        int width = 200;
        int height = 155;
        int padding = 10;
        Rect rect = new Rect(Screen.width - width - padding, padding, width, height);

        // Box mit den Werten zeichnen
        GUI.Box(rect, "", style);

        GUILayout.BeginArea(rect);
        GUILayout.Label($"<b>Dog Stats:</b>", style);
        GUILayout.Label($"Stamina: {stamina.Value:F1}", style);
        GUILayout.Label($"Hunger: {hunger.Value:F1}", style);
        GUILayout.Label($"Thirst: {thirst.Value:F1}", style);
        GUILayout.Label($"Boredom: {boredom.Value:F1}", style);
        GUILayout.EndArea();
    }
#endif
}