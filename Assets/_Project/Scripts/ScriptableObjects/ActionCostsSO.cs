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
        public float diggingDefaultCosts;

        public DogSO dog;

        private void OnEnable() {
            Sleep = sleepDefaultCosts;
            Rest = restDefaultCosts;
            SeekAttention = seekAttentionDefaultCosts;
            Rage = rageDefaultCosts;
            Digging = diggingDefaultCosts;
        }

        public void UpdateCosts() {
            Sleep = CalculateGoodBehaviourActionCost(sleepDefaultCosts, dog.Stamina, dog.Aggression);
            Rest = CalculateBadBehaviourActionCost(restDefaultCosts, dog.Stamina, dog.Aggression);
            SeekAttention = CalculateGoodBehaviourActionCost(seekAttentionDefaultCosts, dog.Fun, dog.Aggression);
            Rage = CalculateBadBehaviourActionCost(rageDefaultCosts, dog.Fun, dog.Aggression);
            Digging = CalculateBadBehaviourActionCost(diggingDefaultCosts, dog.Fun, dog.Aggression);       
        }
        
        private float CalculateGoodBehaviourActionCost(float defaultCost, float currentStatValue, float currentAggression){
            float actionCost = defaultCost - (currentStatValue / 10f) + (currentAggression / 10);
            // actionCost = Random.Range(actionCost - 5, actionCost + 5); 
            return actionCost;       
        }

        private float CalculateBadBehaviourActionCost(float defaultCost, float currentStatValue, float currentAggression) {
            float actionCost = defaultCost + (currentStatValue / 10f) - (currentAggression / 10);
            // actionCost = Random.Range(actionCost - 5, actionCost + 5); 
            return actionCost;  
        }
    }
}