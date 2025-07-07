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
        private float idleDefaultCosts;
        
        [FoldoutGroup("Current Values")]
        public float Wander;
        [FoldoutGroup("Settings"), SerializeField]
        private float wanderDefaultCosts;
        
        [FoldoutGroup("Current Values")]
        public float KeepThirstLevelUp;
        [FoldoutGroup("Settings"), SerializeField]
        private float keepThirstLevelUpDefaultCosts;
        
        [FoldoutGroup("Current Values")]
        public float KeepHungerLevelUp;
        [FoldoutGroup("Settings"), SerializeField]
        private float keepHungerLevelUpDefaultCosts;
        
        [FoldoutGroup("Current Values")]
        public float KeepStaminaUp;
        [FoldoutGroup("Settings"), SerializeField]
        private float keepStaminaUpDefaultCosts;
        
        [FoldoutGroup("Current Values")]
        public float KeepBoredomLow;
        [FoldoutGroup("Settings"), SerializeField]
        private float keepBoredomLowDefaultCosts;
        
        // [FoldoutGroup("Current Values")]
        // public float GetAttention;
        // [FoldoutGroup("Settings"), SerializeField]
        // private float getAttentionDefaultCosts;
        
        [FoldoutGroup("Current Values")]
        public float FetchBallAndReturnIt;
        [FoldoutGroup("Settings"), SerializeField]
        private float fetchBallAndReturnItDefaultCosts;

        private void OnEnable() {
            Idle = idleDefaultCosts;
            Wander = wanderDefaultCosts;
            KeepThirstLevelUp = keepThirstLevelUpDefaultCosts;
            KeepHungerLevelUp = keepHungerLevelUpDefaultCosts;
            KeepStaminaUp = keepStaminaUpDefaultCosts;
            KeepBoredomLow = keepBoredomLowDefaultCosts;
            // GetAttention = getAttentionDefaultCosts;
            FetchBallAndReturnIt = fetchBallAndReturnItDefaultCosts;
        }
    }
}