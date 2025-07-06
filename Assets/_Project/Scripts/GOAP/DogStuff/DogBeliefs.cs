// using System;
// using System.Collections.Generic;
// using ImprovedTimers;
// using ScriptableValues;
// using Sirenix.OdinInspector;
// using UnityEngine;
// using UnityEngine.AI;
//
// public class DogBeliefs : MonoBehaviour
// {
//     [FoldoutGroup("Stats", expanded: true), SerializeField, InlineEditor]
//     private ScriptableFloatValue stamina;
//
//     [FoldoutGroup("Stats"), SerializeField, InlineEditor]
//     private ScriptableFloatValue thirst;
//
//     [FoldoutGroup("Stats"), SerializeField, InlineEditor]
//     private ScriptableFloatValue hunger;
//
//     [FoldoutGroup("Stats"), SerializeField, InlineEditor]
//     private ScriptableFloatValue boredom;
//
//     [FoldoutGroup("Stats"), SerializeField, InlineEditor]
//     private ScriptableBoolValue ballInHand;
//
//     [FoldoutGroup("Stats"), SerializeField, InlineEditor]
//     private ScriptableBoolValue ballThrown;
//
//     [FoldoutGroup("Stats"), SerializeField, InlineEditor]
//     private ScriptableBoolValue ballReturned;
//
//     // [FoldoutGroup("Agent Parameters", expanded: false), InlineEditor] [SerializeField]
//     // private DogParamsSO dogParams;
//
//     [FoldoutGroup("Sensors", expanded: true), SerializeField]
//     private Sensor chaseSensor;
//
//     [FoldoutGroup("Sensors"), SerializeField]
//     private Sensor attackSensor;
//
//     [FoldoutGroup("Known Locations", expanded: true), SerializeField]
//     private Transform restingPosition;
//
//     [FoldoutGroup("Known Locations"), SerializeField]
//     private Transform foodBowl;
//
//     [FoldoutGroup("Known Locations"), SerializeField]
//     private Transform waterBowl;
//
//     [FoldoutGroup("Known Locations"), SerializeField]
//     private Transform rageVictim;
//
//     [FoldoutGroup("Known Locations"), SerializeField]
//     private Transform doorOnePosition;
//
//     [FoldoutGroup("Known Locations"), SerializeField]
//     private Transform doorTwoPosition;
//
//     [FoldoutGroup("Known Locations"), SerializeField]
//     private Transform playerPosition;
//
//     // [FoldoutGroup("Known Locations"), SerializeField]
//     // private Transform ballPosition;
//
//     [FoldoutGroup("Known Locations"), SerializeField]
//     private Transform objectGrabPointPosition;
//
//     public GameObject ball;
//
//     private NavMeshAgent navMeshAgent;
//     private AnimationController animations;
//     private Rigidbody rb;
//     private CountdownTimer statsTimer;
//     private GameObject target;
//     private Vector3 destination;
//
//     private Dictionary<Beliefs, AgentBelief> beliefs;
//     private AgentGoal currentGoal;
//     private AgentAction currentAction;
//     
//     
//
//     private GoapAgent agent;
//     
//
//     public DogBeliefs(GoapAgent agent, NavMeshAgent navMeshAgent, AnimationController animations) {
//         this.agent = agent;
//     }
//
//     private void Awake() {
//         navMeshAgent = GetComponent<NavMeshAgent>();
//         animations = GetComponent<AnimationController>();
//     }
//
//     void OnEnable() => chaseSensor.OnTargetChanged += HandleTargetChanged;
//
//     void OnDisable() => chaseSensor.OnTargetChanged -= HandleTargetChanged;
//
//     
//   A
// }