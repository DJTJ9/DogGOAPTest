using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableValues
{
    [ManageableData]
    [CreateAssetMenu(fileName = "Action Costs", menuName = "Scriptable Objects/Action Costs")]
    public class ActionCostsSO : ScriptableObject
    {
        [FoldoutGroup("Current Values", expanded: true)]
        public float Sleep;
        [FoldoutGroup("Settings", expanded: false), SerializeField]
        private float sleepDefaultCosts;
        
        [FoldoutGroup("Current Values")]
        public float Rest;
        [FoldoutGroup("Settings"), SerializeField]
        private float wanderDefaultCosts;
        
        [FoldoutGroup("Current Values")]
        public float Attention;
        [FoldoutGroup("Settings"), SerializeField]
        private float attentionDefaultCosts;
        
        [FoldoutGroup("Current Values")]
        public float Rage;
        [FoldoutGroup("Settings"), SerializeField]
        private float rageDefaultCosts;
        
        // [FoldoutGroup("Current Values")]
        // public float KeepStaminaUp;
        // [FoldoutGroup("Settings"), SerializeField]
        // private float keepStaminaUpDefaultCosts;
        //
        // [FoldoutGroup("Current Values")]
        // public float KeepBoredomLow;
        // [FoldoutGroup("Settings"), SerializeField]
        // private float keepBoredomLowDefaultCosts;
        //
        // [FoldoutGroup("Current Values")]
        // public float GetAttention;
        // [FoldoutGroup("Settings"), SerializeField]
        // private float getAttentionDefaultCosts;
        //
        // [FoldoutGroup("Current Values")]
        // public float FetchBallAndReturnIt;
        // [FoldoutGroup("Settings"), SerializeField]
        // private float fetchBallAndReturnItDefaultCosts;

        private void OnEnable() {
            Sleep = sleepDefaultCosts;
            Rest = wanderDefaultCosts;
            Attention = attentionDefaultCosts;
            Rage = rageDefaultCosts;
            // KeepStaminaUp = keepStaminaUpDefaultCosts;
            // KeepBoredomLow = keepBoredomLowDefaultCosts;
            // GetAttention = getAttentionDefaultCosts;
            // FetchBallAndReturnIt = fetchBallAndReturnItDefaultCosts;
        }
    }
}