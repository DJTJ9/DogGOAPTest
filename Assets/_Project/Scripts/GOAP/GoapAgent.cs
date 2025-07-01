using System;
using System.Collections.Generic;
using System.Linq;
using ImprovedTimers;
using ScriptableValues;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AnimationController))]
public class GoapAgent : MonoBehaviour
{
    #region Class Members
    // [InlineEditor, AssetList(Path = "/_Project/Scriptable Objects/Scriptable Values", AutoPopulate = true)]
    // public List<ScriptableFloatValue> floatValues;
    
    [FoldoutGroup("Agent Parameters", expanded: true)] [SerializeField]
    private float wanderRadius = 20f;
    [FoldoutGroup("Agent Parameters")] [SerializeField]
    private float pickUpDistance = 2f;
    [FoldoutGroup("Agent Parameters")] [SerializeField]
    private float restingDuration = 5f;

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

    [FoldoutGroup("Sensors", expanded: true), SerializeField] 
    private Sensor chaseSensor;
    [FoldoutGroup("Sensors"), SerializeField] 
    private Sensor attackSensor;

    [FoldoutGroup("Known Locations", expanded: true), SerializeField]
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

    // [Header("Strategy Scriptable Objects")]
    // [SerializeField] private IdleStrategyScriptableObject idleStrategy;
    // [SerializeField] private WanderStrategyScriptableObject wanderStrategy;
    // [SerializeField] private MoveStrategyScriptableObject moveStrategy;
    // [SerializeField] private AttackStrategyScriptableObject attackStrategy;
    // [SerializeField] private EatStrategyScriptableObject eatStrategy;
    // [SerializeField] private DrinkStrategyScriptableObject drinkStrategy;

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

    private int sleepCost = 1;
    private int restCost = 1;
    
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
        SetupBeliefs();
        SetupActions();
        SetupGoals();
    }

    void OnEnable() => chaseSensor.OnTargetChanged += HandleTargetChanged;

    void OnDisable() => chaseSensor.OnTargetChanged -= HandleTargetChanged;
    
    void SetupBeliefs() {
            beliefs = new Dictionary<Beliefs, AgentBelief>();
        BeliefFactory factory = new BeliefFactory(this, beliefs);

        factory.AddBelief(Beliefs.Nothing, () => false);
        factory.AddBelief(Beliefs.DogIdle, () => !navMeshAgent.hasPath);
        factory.AddBelief(Beliefs.DogMoving, () => navMeshAgent.hasPath);
        factory.AddBelief(Beliefs.DogStaminaLow, () => stamina.Value < 10);
        factory.AddBelief(Beliefs.DogIsRested, () => stamina.Value >= 30);
        factory.AddBelief(Beliefs.DogHungerLow, () => hunger.Value < 10);
        factory.AddBelief(Beliefs.DogIsFed, () => hunger.Value >= 30);
        factory.AddBelief(Beliefs.DogThirstLevelLow, () => thirst.Value < 10);
        factory.AddBelief(Beliefs.DogIsNotThirsty, () => thirst.Value >= 30);
        factory.AddBelief(Beliefs.DogIsHappy, () => boredom.Value < 90);
        factory.AddBelief(Beliefs.DogWantsAttention, () => boredom.Value is > 50 and <= 90);
        factory.AddBelief(Beliefs.DogIsBored, () => boredom.Value >= 50);
        

        factory.AddLocationBelief(Beliefs.DogAtDoorOne, 3f, doorOnePosition);
        factory.AddLocationBelief(Beliefs.DogAtDoorTwo, 3f, doorTwoPosition);
        factory.AddLocationBelief(Beliefs.DogAtRestingPosition, 3f, restingPosition);
        factory.AddLocationBelief(Beliefs.DogAtFoodBowl, 3f, foodBowl);
        factory.AddLocationBelief(Beliefs.DogAtWaterBowl, 3f, waterBowl);
        factory.AddLocationBelief(Beliefs.DogAtRageVictim, 3f, rageVictim);

        factory.AddSensorBelief(Beliefs.PlayerInChaseRange, chaseSensor);
        factory.AddSensorBelief(Beliefs.PlayerInAttackRange, attackSensor);
        factory.AddBelief(Beliefs.AttackingPlayer, () => false); // Player can always be attacked, this will never become true
        factory.AddBelief(Beliefs.AttackingRageVictim, () => false);
    }

    protected virtual void SetupActions() {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder("Relax")
            .WithStrategy(new IdleStrategy(restingDuration, stamina))
            .AddEffect(beliefs[Beliefs.Nothing])
            .Build());

        actions.Add(new AgentAction.Builder("WanderAround")
            .WithStrategy(new WanderStrategy(navMeshAgent, wanderRadius))
            .AddEffect(beliefs[Beliefs.DogMoving])
            .Build());

        actions.Add(new AgentAction.Builder("MoveFromRelaxToRestArea")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position, 2.0f))
            .AddPrecondition(beliefs[Beliefs.DogIdle])
            .AddEffect(beliefs[Beliefs.DogAtRestingPosition])
            .Build());

        actions.Add(new AgentAction.Builder("MoveFromWanderToRestArea")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position, 2.0f))
            .AddPrecondition(beliefs[Beliefs.DogMoving])
            .AddEffect(beliefs[Beliefs.DogAtRestingPosition])
            .Build());

        actions.Add(new AgentAction.Builder("MoveToEatingPosition")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => foodBowl.position, pickUpDistance))
            .AddEffect(beliefs[Beliefs.DogAtFoodBowl])
            .Build());

        actions.Add(new AgentAction.Builder("Eat")
            .WithStrategy(
                new EatAndWaitStrategy(animations, hunger))
            .AddPrecondition(beliefs[Beliefs.DogAtFoodBowl])
            .AddEffect(beliefs[Beliefs.DogIsFed])
            .Build());

        actions.Add(new AgentAction.Builder("MoveFromFoodBowlToRestArea")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position, 2.0f))
            .AddPrecondition(beliefs[Beliefs.DogAtFoodBowl])
            .AddEffect(beliefs[Beliefs.DogAtRestingPosition])
            .Build());

        actions.Add(new AgentAction.Builder("MoveToDrinkingPosition")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => waterBowl.position, pickUpDistance))
            .AddEffect(beliefs[Beliefs.DogAtWaterBowl])
            .Build());

        actions.Add(new AgentAction.Builder("Drink")
            .WithStrategy(new DrinkAndWaitStrategy(animations, thirst))
            .AddPrecondition(beliefs[Beliefs.DogAtWaterBowl])
            .AddEffect(beliefs[Beliefs.DogIsNotThirsty])
            .Build());

        actions.Add(new AgentAction.Builder("MoveFromWaterBowlToRestArea")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position, 2.0f))
            .AddPrecondition(beliefs[Beliefs.DogAtWaterBowl])
            .AddEffect(beliefs[Beliefs.DogAtRestingPosition])
            .Build());

        actions.Add(new AgentAction.Builder("MoveToDoorOne")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => doorOnePosition.position))
            .AddEffect(beliefs[Beliefs.DogAtDoorOne])
            .Build());

        actions.Add(new AgentAction.Builder("MoveToDoorTwo")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => doorTwoPosition.position))
            .AddEffect(beliefs[Beliefs.DogAtDoorTwo])
            .Build());

        actions.Add(new AgentAction.Builder("MoveFromDoorOneToRestArea")
            // .WithCost(2)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position, 2.0f))
            .AddPrecondition(beliefs[Beliefs.DogAtDoorOne])
            .AddEffect(beliefs[Beliefs.DogAtRestingPosition])
            .Build());

        actions.Add(new AgentAction.Builder("MoveFromDoorTwoToRestArea")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position, 2.0f))
            .AddPrecondition(beliefs[Beliefs.DogAtDoorTwo])
            .AddEffect(beliefs[Beliefs.DogAtRestingPosition])
            .Build());
        
        actions.Add(new AgentAction.Builder("Sleep")
            .WithCost(sleepCost)
            .WithStrategy(new SleepAndWaitStrategy(animations, stamina))
            .AddPrecondition(beliefs[Beliefs.DogAtRestingPosition])
            .AddEffect(beliefs[Beliefs.DogIsRested])
            .Build());

        actions.Add(new AgentAction.Builder("Rest")
            .WithCost(restCost)
            .WithStrategy(new IdleStrategy(restingDuration, stamina))
            .AddPrecondition(beliefs[Beliefs.DogIdle])
            .AddEffect(beliefs[Beliefs.DogIsRested])
            .Build());

        actions.Add(new AgentAction.Builder("ChasePlayer")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => beliefs[Beliefs.PlayerInChaseRange].Location, 1.5f))
            .AddPrecondition(beliefs[Beliefs.PlayerInChaseRange])
            .AddEffect(beliefs[Beliefs.PlayerInAttackRange])
            .Build());

        actions.Add(new AgentAction.Builder("AttackPlayer")
            .WithStrategy(new AttackStrategy(animations, boredom))
            .AddPrecondition(beliefs[Beliefs.PlayerInAttackRange])
            .AddEffect(beliefs[Beliefs.DogWantsAttention])
            .Build());

        actions.Add(new AgentAction.Builder("MoveFromRelaxToRageVictim")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => rageVictim.position, 2.2f))
            .AddPrecondition(beliefs[Beliefs.DogIdle])
            .AddEffect(beliefs[Beliefs.DogAtRageVictim])
            .Build());
        
        actions.Add(new AgentAction.Builder("AttackRageVictim")
            .WithStrategy(new AttackStrategy(animations, boredom))
            .AddPrecondition(beliefs[Beliefs.DogAtRageVictim])
            .AddEffect(beliefs[Beliefs.DogIsHappy])
            .Build());
    }

    void SetupGoals() {
        goals = new HashSet<AgentGoal>();

        goals.Add(new AgentGoal.Builder("Idle")
            .WithPriority(1)
            .WithDesiredEffect(beliefs[Beliefs.Nothing])
            .Build());

        goals.Add(new AgentGoal.Builder("Wander")
            .WithPriority(2)
            .WithDesiredEffect(beliefs[Beliefs.DogMoving])
            .Build());

        goals.Add(new AgentGoal.Builder("KeepThirstLevelUp")
            .WithPriority(3)
            .WithDesiredEffect(beliefs[Beliefs.DogIsNotThirsty])
            .Build());

        goals.Add(new AgentGoal.Builder("KeepHungerLevelUp")
            .WithPriority(3)
            .WithDesiredEffect(beliefs[Beliefs.DogIsFed])
            .Build());

        goals.Add(new AgentGoal.Builder("KeepStaminaUp")
            .WithPriority(3)
            .WithDesiredEffect(beliefs[Beliefs.DogIsRested])
            .Build());

        goals.Add(new AgentGoal.Builder("AttackPlayer")
            .WithPriority(3)
            .WithDesiredEffect(beliefs[Beliefs.AttackingPlayer])
            .Build());

        goals.Add(new AgentGoal.Builder("KeepBoredomLow")
            .WithPriority(3)
            .WithDesiredEffect(beliefs[Beliefs.DogIsHappy])
            .Build());

        goals.Add(new AgentGoal.Builder("GetAttention")
            .WithPriority(4)
            .WithDesiredEffect(beliefs[Beliefs.DogWantsAttention])
            .Build());
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
        // sleepCost = Random.Range(2, 6);
        // restCost = Random.Range(1, 5);
        // SetupActions();
        
        hunger.Subtract(1);
        boredom.Add(1); 
        stamina.Subtract(1);
        thirst.Subtract(1);
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

        // Update the plan and current action if there is one
        if (currentAction == null) {
            Debug.Log("Calculating any potential new plan");
            CalculatePlan();

            if (actionPlan != null && actionPlan.Actions.Count > 0) {
                navMeshAgent.ResetPath();

                currentGoal = actionPlan.AgentGoal;
                Debug.Log($"Goal: {currentGoal.Name} with {actionPlan.Actions.Count} actions in plan");
                currentAction = actionPlan.Actions.Pop();
                Debug.Log($"Popped action: {currentAction.Name}");
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
                Debug.Log($"{currentAction.Name} complete");
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