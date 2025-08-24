using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableValues
{
    [InlineEditor(Expanded = true)]
    [CreateAssetMenu(fileName = "Action Costs", menuName = "Scriptable Objects/Action Costs")]
    public class ActionCostsSO : ScriptableObject
    {
        [FoldoutGroup("Current Values", expanded: true)]
        public float Sleep;
        [FoldoutGroup("Settings", expanded: false), SerializeField, Title("Sleep")]
        public float sleepDefaultCosts;
        
        [FoldoutGroup("Current Values")]
        public float Rest;
        [FoldoutGroup("Settings"), SerializeField, Title("Rest")]
        public float restDefaultCosts;
        
        [FoldoutGroup("Current Values")]
        public float SeekAttention;
        [FoldoutGroup("Settings"), SerializeField, Title("Attention")]
        public float seekAttentionDefaultCosts;
        
        [FoldoutGroup("Current Values")]
        public float Rage;
        [FoldoutGroup("Settings"), SerializeField, Title("Rage")]
        public float rageDefaultCosts;

        [FoldoutGroup("Current Values")]
        public float Digging;
        [FoldoutGroup("Settings"), SerializeField, Title("Digging")]
        private float diggingDefaultCosts;

        public DogSO dog;

        private void OnEnable() {
            Sleep = sleepDefaultCosts;
            Rest = restDefaultCosts;
            SeekAttention = seekAttentionDefaultCosts;
            Rage = rageDefaultCosts;
            Digging = diggingDefaultCosts;
        }
    }
}