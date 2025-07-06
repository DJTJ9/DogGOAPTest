using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableValues
{
    [ManageableData, InlineEditor(Expanded = true)]
    [CreateAssetMenu(fileName = "Action Costs", menuName = "Scriptable Objects/Action Costs")]
    public class ActionCostsSO : ScriptableObject
    {
        [FoldoutGroup("Current Values", expanded: true)]
        public float Sleep;
        [FoldoutGroup("Settings", expanded: false), SerializeField, Title("Sleep")]
        private float sleepDefaultCosts;
        [FoldoutGroup("Settings"), SerializeField]
        public float minSleepCosts;
        [FoldoutGroup("Settings"), SerializeField]
        public float maxSleepCosts;
        
        [FoldoutGroup("Current Values")]
        public float Rest;
        [FoldoutGroup("Settings"), SerializeField, Title("Rest")]
        private float restDefaultCosts;
        [FoldoutGroup("Settings"), SerializeField]
        public float minRestCosts;
        [FoldoutGroup("Settings"), SerializeField]
        public float maxRestCosts;
        
        [FoldoutGroup("Current Values")]
        public float Attention;
        [FoldoutGroup("Settings"), SerializeField, Title("Attention")]
        private float attentionDefaultCosts;
        [FoldoutGroup("Settings"), SerializeField]
        public float minAttentionCosts;
        [FoldoutGroup("Settings"), SerializeField]
        public float maxAttentionCosts;
        
        [FoldoutGroup("Current Values")]
        public float Rage;
        [FoldoutGroup("Settings"), SerializeField, Title("Rage")]
        private float rageDefaultCosts;
        [FoldoutGroup("Settings"), SerializeField]
        public float minRageCosts;
        [FoldoutGroup("Settings"), SerializeField]
        public float maxRageCosts;

        private void OnEnable() {
            Sleep = sleepDefaultCosts;
            Rest = restDefaultCosts;
            Attention = attentionDefaultCosts;
            Rage = rageDefaultCosts;
        }
    }
}