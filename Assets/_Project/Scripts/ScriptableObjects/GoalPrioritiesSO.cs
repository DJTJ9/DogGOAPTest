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
        public float KeepStaminaUp;
        [FoldoutGroup("Settings"), SerializeField]
        private float keepStaminaUpDefaultPriority;
        
        [FoldoutGroup("Current Values")]
        public float KeepHydrationLevelUp;
        [FoldoutGroup("Settings"), SerializeField]
        private float keepHydrationLevelUpDefaultPriority;
        
        [FoldoutGroup("Current Values")]
        public float KeepSatietyLevelUp;
        [FoldoutGroup("Settings"), SerializeField]
        private float keepHungerLevelUpDefaultPriority;
        
        [FoldoutGroup("Current Values")]
        public float KeepFunUp;
        [FoldoutGroup("Settings"), SerializeField]
        private float keepBoredomLowDefaultPriority;
        
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
            KeepStaminaUp = keepStaminaUpDefaultPriority;
            KeepHydrationLevelUp = keepHydrationLevelUpDefaultPriority;
            KeepSatietyLevelUp = keepHungerLevelUpDefaultPriority;
            KeepFunUp = keepBoredomLowDefaultPriority;
            FetchBallAndReturnIt = fetchBallAndReturnItDefaultPriority;
            StayAlive = stayAliveDefaultPriority;
        }
        
        // public void UpdatePriorities() {
        //     KeepStaminaUp = CalculatePriorities(keepStaminaUpDefaultPriority, DogSO.Stamina);
        //     KeepHydrationLevelUp = CalculatePriorities(keepHydrationLevelUpDefaultPriority, DogSO.Satiety);
        //     KeepSatietyLevelUp = CalculatePriorities(keepHungerLevelUpDefaultPriority, DogSO.Hydration);
        //     KeepFunUp = CalculatePriorities(keepBoredomLowDefaultPriority, DogSO.Fun);
        //     FetchBallAndReturnIt = CalculatePriorities(fetchBallAndReturnItDefaultPriority, DogSO.Fun);
        // }
        
        private float CalculatePriorities(float defaultPriority, float currentStatValue) {
            float goalPriority = defaultPriority - (currentStatValue / 10f);
            return goalPriority;
        //     KeepStaminaUp = defaultPriority - (currentStatValue / 10f);
        //     KeepThirstLevelUp = defaultPriority - (currentStatValue / 10f);
        //     KeepHungerLevelUp = defaultPriority - (currentStatValue / 10f);
        //     KeepBoredomLow = defaultPriority - (currentStatValue / 10f);
        //     FetchBallAndReturnIt = defaultPriority - (currentStatValue / 10f);
        //     StayAlive = defaultPriority - (currentStatValue / 10f);       
        }
    }
}