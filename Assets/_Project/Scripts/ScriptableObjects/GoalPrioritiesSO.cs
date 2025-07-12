using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableValues
{
    [ManageableData, InlineEditor(Expanded = true)]
    [CreateAssetMenu(fileName = "Goal Priorities", menuName = "Scriptable Objects/Goal Priorities")]
    public class GoalPrioritiesSO : ScriptableObject
    {
        [FoldoutGroup("Current Values", expanded: true)]
        public float Idle;
        [FoldoutGroup("Settings", expanded: false), SerializeField]
        private float idleDefaultPriority;
        
        [FoldoutGroup("Current Values")]
        public float Wander;
        [FoldoutGroup("Settings"), SerializeField]
        private float wanderDefaultPriority;
        
        [FoldoutGroup("Current Values")]
        public float KeepThirstLevelUp;
        [FoldoutGroup("Settings"), SerializeField]
        private float keepThirstLevelUpDefaultPriority;
        
        [FoldoutGroup("Current Values")]
        public float KeepHungerLevelUp;
        [FoldoutGroup("Settings"), SerializeField]
        private float keepHungerLevelUpDefaultPriority;
        
        [FoldoutGroup("Current Values")]
        public float KeepStaminaUp;
        [FoldoutGroup("Settings"), SerializeField]
        private float keepStaminaUpDefaultPriority;
        
        [FoldoutGroup("Current Values")]
        public float KeepBoredomLow;
        [FoldoutGroup("Settings"), SerializeField]
        private float keepBoredomLowDefaultPriority;
        
        // [FoldoutGroup("Current Values")]
        // public float GetAttention;
        // [FoldoutGroup("Settings"), SerializeField]
        // private float getAttentionDefaultCosts;
        
        [FoldoutGroup("Current Values")]
        public float FetchBallAndReturnIt;
        [FoldoutGroup("Settings"), SerializeField]
        private float fetchBallAndReturnItDefaultPriority;
        
        [FoldoutGroup("Current Values")]
        public float StayAlive;
        [FoldoutGroup("Settings"), SerializeField]
        private float stayAliveDefaultPriority;

        private void OnEnable() {
            Idle = idleDefaultPriority;
            Wander = wanderDefaultPriority;
            KeepThirstLevelUp = keepThirstLevelUpDefaultPriority;
            KeepHungerLevelUp = keepHungerLevelUpDefaultPriority;
            KeepStaminaUp = keepStaminaUpDefaultPriority;
            KeepBoredomLow = keepBoredomLowDefaultPriority;
            // GetAttention = getAttentionDefaultCosts;
            FetchBallAndReturnIt = fetchBallAndReturnItDefaultPriority;
            StayAlive = stayAliveDefaultPriority;
        }
    }
}