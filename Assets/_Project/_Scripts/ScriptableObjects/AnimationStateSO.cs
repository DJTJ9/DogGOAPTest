// using UnityEngine;
//
// namespace ScriptableValues
// {
//     public enum AnimationState
//     {
//         Idle,
//         Walk,
//         Run,
//         Eat,
//         Drink,
//         Sleep,
//         Beg,
//     }
//     
//     [CreateAssetMenu(fileName = "AnimationStateSO", menuName = "Scriptable Objects/Animation State")]
//     public class AnimationStateSO : ScriptableObject
//     {
//         [SerializeField] private AnimationController animator;
//         private int state;
//         private int currentState = (int) AnimationState.Idle;
//         
//         private void UpdateState()
//         {
//             state = GetState();
//             
//             if (state == currentState) return;
//             
//             
//             currentState = state;
//         }
//
//         private int GetState() {
//             UpdateState();
//
//             //Anstatt über AnimationState über einen bool switchen (z.B. isEating, etc.)
//             return currentState switch
//             {
//                 AnimationState.Idle => animator.locomotionTrigger,
//                 AnimationState.Walk => animator.locomotionTrigger,
//                 AnimationState.Run => animator.locomotionTrigger,
//                 AnimationState.Eat => animator.eatTrigger,
//                 AnimationState.Drink => animator.drinkTrigger,
//                 AnimationState.Sleep => animator.sleepIdleTrigger,
//                 AnimationState.Beg => animator.begTrigger,
//                 _ => animator.locomotionTrigger
//             };
//         }
//     }
// }