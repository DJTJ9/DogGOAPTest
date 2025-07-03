using System;
using System.Collections.Generic;
using ImprovedTimers;
using ScriptableValues;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class DogBeliefs : MonoBehaviour
{
    [FoldoutGroup("Stats", expanded: true), SerializeField, InlineEditor]
    private ScriptableFloatValue stamina;

    [FoldoutGroup("Stats"), ProgressBar(0, 100)]
    public float Stamina => stamina.Value;

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

    // [FoldoutGroup("Agent Parameters", expanded: false), InlineEditor] [SerializeField]
    // private DogParamsSO dogParams;

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

    [FoldoutGroup("Known Locations"), SerializeField]
    private Transform playerPosition;

    // [FoldoutGroup("Known Locations"), SerializeField]
    // private Transform ballPosition;

    [FoldoutGroup("Known Locations"), SerializeField]
    private Transform objectGrabPointPosition;

    public GameObject ball;

    private NavMeshAgent navMeshAgent;
    private AnimationController animations;
    private Rigidbody rb;
    private CountdownTimer statsTimer;
    private GameObject target;
    private Vector3 destination;

    private Dictionary<Beliefs, AgentBelief> beliefs;
    private AgentGoal currentGoal;
    private AgentAction currentAction;
    
    

    private GoapAgent agent;
    

    public DogBeliefs(GoapAgent agent, NavMeshAgent navMeshAgent, AnimationController animations) {
        this.agent = agent;
    }

    private void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animations = GetComponent<AnimationController>();
    }

    void OnEnable() => chaseSensor.OnTargetChanged += HandleTargetChanged;

    void OnDisable() => chaseSensor.OnTargetChanged -= HandleTargetChanged;

    public Dictionary<Beliefs, AgentBelief> SetupBeliefs() {
        beliefs = new Dictionary<Beliefs, AgentBelief>();
        BeliefFactory factory = new BeliefFactory(agent, beliefs);

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
        factory.AddBelief(Beliefs.BallInHand, () => ballInHand.Value);
        ;
        factory.AddBelief(Beliefs.BallThrown, () => ballThrown.Value);
        factory.AddBelief(Beliefs.ReturnBall, () => ballReturned.Value);
        // factory.AddBelief(Beliefs.BallRetrieved, () => !ballThrown.Value && !ballInHand.Value);
        factory.AddBelief(Beliefs.BallReturned, () => ballReturned.Value);
        factory.AddBelief(Beliefs.FetchBall, () => !ballThrown.Value && !ballInHand.Value);
        factory.AddBelief(Beliefs.DropBall, () => false);

        factory.AddLocationBelief(Beliefs.DogAtDoorOne, 3f, doorOnePosition);
        factory.AddLocationBelief(Beliefs.DogAtDoorTwo, 3f, doorTwoPosition);
        factory.AddLocationBelief(Beliefs.DogAtRestingPosition, 3f, restingPosition);
        factory.AddLocationBelief(Beliefs.DogAtFoodBowl, 3f, foodBowl);
        factory.AddLocationBelief(Beliefs.DogAtWaterBowl, 3f, waterBowl);
        factory.AddLocationBelief(Beliefs.DogAtRageVictim, 3f, rageVictim);
        factory.AddLocationBelief(Beliefs.DogAtBall, 2f, ball.transform.position);
        ;

        factory.AddSensorBelief(Beliefs.PlayerInChaseRange, chaseSensor);
        factory.AddSensorBelief(Beliefs.PlayerInAttackRange, attackSensor);
        factory.AddBelief(Beliefs.AttackingPlayer,
            () => false); // Player can always be attacked, this will never become true
        factory.AddBelief(Beliefs.AttackingRageVictim, () => false);
        
        return beliefs;
    }
    
    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(transform.position, pos) < range;
    
    void HandleTargetChanged() {
        Debug.Log("Target changed, clearing current action and goal");
        // Force the planner to re-evaluate the plan
        currentAction = null;
        currentGoal = null;
    }
}