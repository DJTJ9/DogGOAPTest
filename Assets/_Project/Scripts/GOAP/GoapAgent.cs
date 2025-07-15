using System;
using System.Collections.Generic;
using System.Linq;
using ImprovedTimers;
using ScriptableValues;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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

    [FoldoutGroup("Dog Parameters", expanded: false), InlineEditor]
    [SerializeField]
    private DogParamsSO dogParams;

    [FoldoutGroup("Dog Parameters"), InlineEditor, SerializeField]
    private ActionCostsSO actionCosts;

    [FoldoutGroup("Dog Parameters"), InlineEditor, SerializeField]
    private GoalPrioritiesSO goalPriorities;

    [FoldoutGroup("Dog Parameters"), InlineEditor, SerializeField]
    private StatManager statManager;

    [FoldoutGroup("Known Locations", expanded: false), SerializeField]
    private Transform restingPosition;

    [FoldoutGroup("Known Locations"), SerializeField]
    private Transform foodBowl;

    [FoldoutGroup("Known Locations"), SerializeField]
    private Transform waterBowl;

    [FoldoutGroup("Known Locations"), SerializeField]
    private Transform rageVictim;

    [FoldoutGroup("Known Locations"), SerializeField]
    private Transform playerTransform;

    [FoldoutGroup("Known Locations"), SerializeField]
    private Transform objectGrabPointPosition;

    [FoldoutGroup("Known Locations"), SerializeField]
    private GameObject ball;

    [FoldoutGroup("Health Bar", expanded: false), SerializeField]
    private Slider healthBarSlider;
    
    public HashSet<AgentAction> actions;
    
    private NavMeshAgent navMeshAgent;
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

    // void OnEnable() => playerSensor.OnTargetChanged += HandleTargetChanged;
    //
    // void OnDisable() => playerSensor.OnTargetChanged -= HandleTargetChanged;

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
        factory.AddBelief(Beliefs.DogIsHappy, () => dog.Fun >= 30);
        factory.AddBelief(Beliefs.DogIsBored, () => dog.Fun < 70);
        ;
        factory.AddBelief(Beliefs.BallInHand, () => dog.BallInHand);
        factory.AddBelief(Beliefs.FetchBall, () => dog.FetchBall);
        factory.AddBelief(Beliefs.BallThrown, () => dog.BallThrown);
        factory.AddBelief(Beliefs.ReturnBall, () => dog.ReturnBall);
        factory.AddBelief(Beliefs.BallReturned, () => dog.BallReturned);
        factory.AddBelief(Beliefs.DropBall, () => false);
        
        factory.AddBelief(Beliefs.FoodIsAvailable, () => dog.FoodAvailable);
        factory.AddBelief(Beliefs.WaterIsAvailable, () => dog.WaterAvailable.Value);
        factory.AddBelief(Beliefs.RestingSpotIsAvailable, () => dog.RestingSpotAvailable);

        factory.AddLocationBelief(Beliefs.DogAtRestingPosition, 3f, restingPosition);
        factory.AddLocationBelief(Beliefs.DogAtFoodBowl, 2.2f, foodBowl);
        factory.AddLocationBelief(Beliefs.DogAtWaterBowl, 2.2f, waterBowl);
        factory.AddLocationBelief(Beliefs.DogAtRageVictim, 2.2f, rageVictim);
        factory.AddLocationBelief(Beliefs.DogAtBall, 2.2f, ball.transform.position);
        factory.AddLocationBelief(Beliefs.DogAtPlayer, 4f, playerTransform.position);
        
        factory.AddBelief(Beliefs.AttackingRageVictim, () => false);
        factory.AddBelief(Beliefs.Attacking, () => false);
    }
    
    [Button("Update Actions"), FoldoutGroup("Buttons"), PropertyOrder(-10)]
    protected virtual void SetupActions() {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder(ActionType.Relax)
            .WithStrategy(new IdleStrategy(dogParams.restingDuration, dog))
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

        actions.Add(new AgentAction.Builder(ActionType.MoveToEatingPosition)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => foodBowl.position, dogParams.pickUpDistance))
            .AddEffect(beliefs[Beliefs.DogAtFoodBowl])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.Eat)
            .WithStrategy(new EatStrategy(animations, foodBowl, dog))
            .AddPrecondition(beliefs[Beliefs.FoodIsAvailable])
            .AddPrecondition(beliefs[Beliefs.DogAtFoodBowl])
            .AddPrecondition(beliefs[Beliefs.DogIsHungry])
            .AddEffect(beliefs[Beliefs.DogIsNotHungry])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.MoveToDrinkingPosition)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => waterBowl.position, dogParams.pickUpDistance))
            .AddEffect(beliefs[Beliefs.DogAtWaterBowl])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.Drink)
            .WithStrategy(new DrinkStrategy(animations, waterBowl, dog))
            .AddPrecondition(beliefs[Beliefs.WaterIsAvailable])
            .AddPrecondition(beliefs[Beliefs.DogAtWaterBowl])
            .AddPrecondition(beliefs[Beliefs.DogIsThirsty])
            .AddEffect(beliefs[Beliefs.DogIsNotThirsty])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.Sleep)
            .WithCost(actionCosts.Sleep)
            .WithStrategy(new SleepAndWaitStrategy(animations, dog))
            .AddPrecondition(beliefs[Beliefs.RestingSpotIsAvailable])
            .AddPrecondition(beliefs[Beliefs.DogAtRestingPosition])
            .AddPrecondition(beliefs[Beliefs.DogIsExhausted])
            .AddEffect(beliefs[Beliefs.DogIsRested])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.Rest)
            .WithCost(actionCosts.Rest)
            .WithStrategy(new IdleStrategy(dogParams.restingDuration, dog, 50f))
            .AddPrecondition(beliefs[Beliefs.RestingSpotIsAvailable])
            .AddPrecondition(beliefs[Beliefs.DogAtRestingPosition])
            .AddPrecondition(beliefs[Beliefs.DogIsExhausted])
            .AddEffect(beliefs[Beliefs.DogIsRested])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.MoveToPlayer)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => playerTransform.position, 4f))
            .AddEffect(beliefs[Beliefs.DogAtPlayer])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.SeekAttention)
            .WithCost(actionCosts.SeekAttention)
            .WithStrategy(new SeekAttentionStrategy(navMeshAgent, animations, playerTransform, dog))
            .AddPrecondition(beliefs[Beliefs.DogIsBored])
            .AddPrecondition(beliefs[Beliefs.DogAtPlayer])
            .AddEffect(beliefs[Beliefs.DogIsHappy])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.MoveToRageVictim)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => rageVictim.position, 3.2f))
            .AddEffect(beliefs[Beliefs.DogAtRageVictim])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.AttackRageVictim)
            .WithCost(actionCosts.Rage)
            .WithStrategy(new AttackStrategy(animations, dog))
            .AddPrecondition(beliefs[Beliefs.DogAtRageVictim])
            .AddPrecondition(beliefs[Beliefs.DogIsBored])
            .AddEffect(beliefs[Beliefs.DogIsHappy])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.Digging)
            .WithCost(actionCosts.Digging)
            .WithStrategy(new DiggingStrategy(animations, dog))
            .AddPrecondition(beliefs[Beliefs.DogIsBored])
            .AddEffect(beliefs[Beliefs.DogIsHappy])
            .Build());

        #region FetchBall
        
        actions.Add(new AgentAction.Builder(ActionType.MoveToPlayerWithBall)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => playerTransform.position, 3f))
            .AddPrecondition(beliefs[Beliefs.BallInHand])
            .AddEffect(beliefs[Beliefs.DogAtPlayer])
            .Build());

        actions.Add(new AgentAction.Builder(ActionType.WaitForBallThrow)
            .AddPrecondition(beliefs[Beliefs.DogAtPlayer])
            .AddEffect(beliefs[Beliefs.FetchBall])
            .Build());
        
        actions.Add(new AgentAction.Builder(ActionType.MoveToBall)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => ball.transform.position))
            .AddPrecondition(beliefs[Beliefs.FetchBall])
            .AddEffect(beliefs[Beliefs.BallThrown])
            .Build());
        
        actions.Add(new AgentAction.Builder(ActionType.PickUpBall)
            .WithStrategy(new PickUpBallStrategy(navMeshAgent, animations, ball, objectGrabPointPosition, dog))
            .AddPrecondition(beliefs[Beliefs.BallThrown])
            .AddEffect(beliefs[Beliefs.ReturnBall])
            .Build());
        
        actions.Add(new AgentAction.Builder(ActionType.ReturnToPlayer)
            .WithStrategy(new MoveStrategy(navMeshAgent, () => playerTransform.position))
            .AddPrecondition(beliefs[Beliefs.ReturnBall])
            .AddEffect(beliefs[Beliefs.DropBall])
            .Build());
        
        actions.Add(new AgentAction.Builder(ActionType.DropBall)
            .WithStrategy(new DropBallStrategy(navMeshAgent, animations, ball, objectGrabPointPosition, dog))
            .AddPrecondition(beliefs[Beliefs.DropBall])
            .AddEffect(beliefs[Beliefs.BallReturned])
            .Build());

        #endregion
    }
    
    [Button("Update Goals"), FoldoutGroup("Buttons"), PropertyOrder(-10)]
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
            .WithDesiredEffect(beliefs[Beliefs.DogIsNotHungry])
            .Build());

        goals.Add(new AgentGoal.Builder(GoalType.KeepExhaustionLow)
            .WithPriority(goalPriorities.KeepStaminaUp)
            .WithDesiredEffect(beliefs[Beliefs.DogIsRested])
            .Build());


        goals.Add(new AgentGoal.Builder(GoalType.KeepBoredomLow)
            .WithPriority(goalPriorities.KeepBoredomLow)
            .WithDesiredEffect(beliefs[Beliefs.DogIsHappy])
            .Build());

        goals.Add(new AgentGoal.Builder(GoalType.FetchBallAndReturnIt)
            .WithPriority(goalPriorities.FetchBallAndReturnIt)
            .WithDesiredEffect(beliefs[Beliefs.BallReturned])
            .Build());

        // goals.Add(new AgentGoal.Builder(GoalType.Attack)
        //     .WithPriority(3)
        //     .WithDesiredEffect(beliefs[Beliefs.Attacking])
        //     .Build());
        
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

        SetupActions();
        SetupGoals();
        actionCosts.UpdateCosts();
        
        if (dog.Stamina <= 0) {
            dog.Health -= 1f;
            dog.Aggression += 1f;
        }
        if (dog.Satiety <= 0) {
            dog.Health -= 1f;
            dog.Aggression += 1f;
        }
        if (dog.Hydration <= 0) {
            dog.Health -= 1f;
            dog.Aggression += 1f;
        }
        
        dog.Health = Mathf.Clamp(dog.Health, 0f, 100f);
        dog.Aggression = Mathf.Clamp(dog.Aggression, 0f, 100f);
        dog.Satiety = Mathf.Clamp(dog.Satiety - statManager.SatietyLost, 0f, 100f);
        dog.Fun = Mathf.Clamp(dog.Fun - statManager.FunLost, 0f, 100f);
        dog.Stamina = Mathf.Clamp(dog.Stamina - statManager.StaminaLost, 0f, 100f);
        dog.Hydration = Mathf.Clamp(dog.Hydration - statManager.HydrationLost, 0f, 100f);

        
        animations.SetAnimatorBool("Death_b", dog.Health <= 0);
    }

    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(transform.position, pos) < range;

    void HandleTargetChanged() {
        Debug.Log("Target changed, clearing current action and goal");
        // Force the planner to re-evaluate the plan
        currentAction = null;
        currentGoal = null;
    }

    void Update() {
        statsTimer.Tick(Time.deltaTime);
        animations.SetSpeed(navMeshAgent.velocity.magnitude);
        healthBarSlider.value = dog.Health;

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
        int height = 220;
        int padding = 10;
        Rect rect = new Rect(Screen.width - width - padding, padding, width, height);

        // Box mit den Werten zeichnen
        GUI.Box(rect, "", style);

        GUILayout.BeginArea(rect);
        GUILayout.Label($"<b>Dog Stats:</b>", style);
        GUILayout.Label($"Health:    {dog.Health:F1}", style);
        GUILayout.Label($"Aggression:{dog.Aggression:F1}", style);
        GUILayout.Label($"Stamina:   {dog.Stamina:F1}", style);
        GUILayout.Label($"Satiety:   {dog.Satiety:F1}", style);
        GUILayout.Label($"Hydration: {dog.Hydration:F1}", style);
        GUILayout.Label($"Fun:       {dog.Fun:F1}", style);
        GUILayout.EndArea();
    }
#endif
}