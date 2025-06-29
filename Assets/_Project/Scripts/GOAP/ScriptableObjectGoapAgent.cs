// using System.Collections.Generic;
// using UnityEngine;
//
// public class ScriptableObjectGoapAgent : GoapAgent
// {
//     [Header("Scriptable Object Configuration")]
//     [SerializeField] private List<ActionSetScriptableObject> actionSets = new List<ActionSetScriptableObject>();
//
//     [Header("Default Sleep Strategy")]
//     [SerializeField] private bool includeSleepStrategy = true;
//
//     protected override void SetupActions()
//     {
//         // Statt der direkten Definition von Actions, laden wir sie aus den Scriptable Objects
//         actions = new HashSet<AgentAction>();
//
//         foreach (var actionSet in actionSets)
//         {
//             LoadActionsFromScriptableObjects(actionSet.Actions);
//         }
//
//         // Füge die Sleep-Action manuell hinzu, wenn gewünscht
//         if (includeSleepStrategy) {
//             actions.Add(new AgentAction.Builder("Sleep")
//                 .WithStrategy(new SleepAndWaitStrategy(GetComponent<AnimationController>()))
//                 .AddPrecondition(beliefs["AgentAtRestingPosition"])
//                 .AddEffect(beliefs["AgentIsRested"])
//                 .Build());
//         }
//     }
// }
