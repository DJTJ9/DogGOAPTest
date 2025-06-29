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

    [Header("Agent Parameters")] [SerializeField]
    private float wanderRadius = 20f;

    [SerializeField] private float pickUpDistance = 2.3f;
    [SerializeField] private float restingDuration = 5f;

    [Header("Sensors")] [SerializeField] private Sensor chaseSensor;
    [SerializeField] private Sensor attackSensor;

    [Header("Known Locations")] [SerializeField]
    private Transform restingPosition;

    [SerializeField] private Transform foodBowl;
    [SerializeField] private Transform waterBowl;
    [SerializeField] private Transform doorOnePosition;
    [SerializeField] private Transform doorTwoPosition;

    [Header("Stats")] 
    [SerializeField, InlineEditor] private ScriptableFloatValue hunger;
    [SerializeField, InlineEditor] private ScriptableFloatValue thirst;
    [SerializeField, InlineEditor] private ScriptableFloatValue stamina;
    [SerializeField, InlineEditor] private ScriptableBoolValue ballInHand;
    [SerializeField, InlineEditor] private ScriptableBoolValue ballThrown;
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
    private Dictionary<string, AgentBelief> beliefs;
    private HashSet<AgentGoal> goals;
    private AgentGoal lastGoal;
    private AgentGoal currentGoal;
    private ActionPlan actionPlan;
    private AgentAction currentAction;


    private IGoapPlanner gPlanner;

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
    
    void OnEnable()  => chaseSensor.OnTargetChanged += HandleTargetChanged;
    
    void OnDisable() => chaseSensor.OnTargetChanged -= HandleTargetChanged;
    
    void SetupBeliefs() {
        beliefs = new Dictionary<string, AgentBelief>();
        BeliefFactory factory = new BeliefFactory(this, beliefs);

        factory.AddBelief("Nothing", () => false);
        factory.AddBelief("AgentIdle", () => !navMeshAgent.hasPath);
        factory.AddBelief("AgentMoving", () => navMeshAgent.hasPath);
        factory.AddBelief("AgentHungerLow", () => hunger.Value < 10);
        factory.AddBelief("AgentIsFed", () => hunger.Value >= 30);
        factory.AddBelief("AgentThirstLevelLow", () => thirst.Value < 10);
        factory.AddBelief("AgentIsNotThirsty", () => thirst.Value >= 30);
        factory.AddBelief("AgentStaminaLow", () => stamina.Value < 20);
        factory.AddBelief("AgentIsRested", () => stamina.Value >= 75);

        factory.AddLocationBelief("AgentAtDoorOne", 3f, doorOnePosition);
        factory.AddLocationBelief("AgentAtDoorTwo", 3f, doorTwoPosition);
        factory.AddLocationBelief("AgentAtRestingPosition", 3f, restingPosition);
        factory.AddLocationBelief("AgentAtFoodBowl", 3f, foodBowl);
        factory.AddLocationBelief("AgentAtWaterBowl", 3f, waterBowl);

        factory.AddSensorBelief("PlayerInChaseRange", chaseSensor);
        factory.AddSensorBelief("PlayerInAttackRange", attackSensor);
        factory.AddBelief("AttackingPlayer", () => false); // Player can always be attacked, this will never become true
    }

    protected virtual void SetupActions() {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder("Relax")
            .WithStrategy(new IdleStrategy(restingDuration))
            .AddEffect(beliefs["Nothing"])
            .Build());

        actions.Add(new AgentAction.Builder("WanderAround")
            .WithStrategy(new WanderStrategy(navMeshAgent, wanderRadius))
            .AddEffect(beliefs["AgentMoving"])
            .Build());
        
        actions.Add(new AgentAction.Builder("MoveFromRelaxToRestArea")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position, 2.0f))
            .AddPrecondition(beliefs["AgentIdle"])
            .AddEffect(beliefs["AgentAtRestingPosition"])
            .Build());
        
        actions.Add(new AgentAction.Builder("MoveFromWanderToRestArea")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position, 2.0f))
            .AddPrecondition(beliefs["AgentMoving"])
            .AddEffect(beliefs["AgentAtRestingPosition"])
            .Build());

        actions.Add(new AgentAction.Builder("MoveToEatingPosition")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => foodBowl.position, pickUpDistance))
            .AddEffect(beliefs["AgentAtFoodBowl"])
            .Build());

        actions.Add(new AgentAction.Builder("Eat")
            .WithStrategy(
                new EatAndWaitStrategy(animations))
            .AddPrecondition(beliefs["AgentAtFoodBowl"])
            .AddEffect(beliefs["AgentIsFed"])
            .Build());
        
        actions.Add(new AgentAction.Builder("MoveFromFoodBowlToRestArea")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position, 2.0f))
            .AddPrecondition(beliefs["AgentAtFoodBowl"])
            .AddEffect(beliefs["AgentAtRestingPosition"])
            .Build());

        actions.Add(new AgentAction.Builder("MoveToDrinkingPosition")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => waterBowl.position, pickUpDistance))
            .AddEffect(beliefs["AgentAtWaterBowl"])
            .Build());

        actions.Add(new AgentAction.Builder("Drink")
            .WithStrategy(new DrinkAndWaitStrategy(animations))
            .AddPrecondition(beliefs["AgentAtWaterBowl"])
            .AddEffect(beliefs["AgentIsNotThirsty"])
            .Build());
        
        actions.Add(new AgentAction.Builder("MoveFromWaterBowlToRestArea")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position, 2.0f))
            .AddPrecondition(beliefs["AgentAtWaterBowl"])
            .AddEffect(beliefs["AgentAtRestingPosition"])
            .Build());

        actions.Add(new AgentAction.Builder("MoveToDoorOne")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => doorOnePosition.position))
            .AddEffect(beliefs["AgentAtDoorOne"])
            .Build());

        actions.Add(new AgentAction.Builder("MoveToDoorTwo")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => doorTwoPosition.position))
            .AddEffect(beliefs["AgentAtDoorTwo"])
            .Build());

        actions.Add(new AgentAction.Builder("MoveFromDoorOneToRestArea")
            // .WithCost(2)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position, 2.0f))
            .AddPrecondition(beliefs["AgentAtDoorOne"])
            .AddEffect(beliefs["AgentAtRestingPosition"])
            .Build());

        actions.Add(new AgentAction.Builder("MoveFromDoorTwoToRestArea")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => restingPosition.position, 2.0f))
            .AddPrecondition(beliefs["AgentAtDoorTwo"])
            .AddEffect(beliefs["AgentAtRestingPosition"])
            .Build());

        actions.Add(new AgentAction.Builder("Sleep")
            .WithStrategy(new SleepAndWaitStrategy(animations))
            .AddPrecondition(beliefs["AgentAtRestingPosition"])
            .AddEffect(beliefs["AgentIsRested"])
            .Build());
        
        actions.Add(new AgentAction.Builder("Rest")
            .WithStrategy(new IdleStrategy(restingDuration))
            .AddPrecondition(beliefs["AgentAtRestingPosition"])
            .AddEffect(beliefs["AgentIsRested"])
            .Build());

        actions.Add(new AgentAction.Builder("ChasePlayer")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => beliefs["PlayerInChaseRange"].Location, 1.5f))
            .AddPrecondition(beliefs["PlayerInChaseRange"])
            .AddEffect(beliefs["PlayerInAttackRange"])
            .Build());

        actions.Add(new AgentAction.Builder("AttackPlayer")
            .WithStrategy(new AttackStrategy(animations))
            .AddPrecondition(beliefs["PlayerInAttackRange"])
            .AddEffect(beliefs["AttackingPlayer"])
            .Build());
    }

    void SetupGoals() {
        goals = new HashSet<AgentGoal>();

        goals.Add(new AgentGoal.Builder("Idle")
            .WithPriority(1)
            .WithDesiredEffect(beliefs["Nothing"])
            .Build());

        goals.Add(new AgentGoal.Builder("Wander")
            .WithPriority(2)
            .WithDesiredEffect(beliefs["AgentMoving"])
            .Build());

        goals.Add(new AgentGoal.Builder("KeepThirstLevelUp")
            .WithPriority(3)
            .WithDesiredEffect(beliefs["AgentIsNotThirsty"])
            .Build());

        goals.Add(new AgentGoal.Builder("KeepHungerLevelUp")
            .WithPriority(3)
            .WithDesiredEffect(beliefs["AgentIsFed"])
            .Build());

        goals.Add(new AgentGoal.Builder("KeepStaminaUp")
            .WithPriority(3)
            .WithDesiredEffect(beliefs["AgentIsRested"])
            .Build());

        goals.Add(new AgentGoal.Builder("AttackPlayer")
            .WithPriority(2)
            .WithDesiredEffect(beliefs["AttackingPlayer"])
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
        if (InRangeOf(restingPosition.position, 2f))
            stamina.Add(10);
        else
            stamina.Subtract(2);

        if (InRangeOf(foodBowl.position, 2f))
            hunger.Add(40);
        else
            hunger.Subtract(1);

        if (InRangeOf(waterBowl.position, 2f))
            thirst.Add(40);
        else
            thirst.Subtract(1);
        
        // stamina += InRangeOf(restingPosition.position, 3f) ? 40 : -20;
        // hunger += InRangeOf(foodBowl.position, 3f) ? 40 : -2;
        // thirst += InRangeOf(waterBowl.position, 3f) ? 40 : -2;
        // stamina = Mathf.Clamp(stamina, 0, 100);
        // hunger = Mathf.Clamp(hunger, 0, 100);
        // thirst = Mathf.Clamp(thirst, 0, 100);
    }

    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(transform.position, pos) < range;

    #if UNITY_EDITOR
    private void OnGUI()
    {
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
        int height = 120;
        int padding = 10;
        Rect rect = new Rect(Screen.width - width - padding, padding, width, height);

        // Box mit den Werten zeichnen
        GUI.Box(rect, "", style);

        GUILayout.BeginArea(rect);
        GUILayout.Label($"<b>Dog Stats:</b>", style);
        GUILayout.Label($"Hunger: {hunger.Value:F1}", style);
        GUILayout.Label($"Thirst: {thirst.Value:F1}", style);
        GUILayout.Label($"Stamina: {stamina.Value:F1}", style);
        GUILayout.EndArea();
    }
    #endif

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
}