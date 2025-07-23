using System;
using ScriptableValues;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Dog", menuName = "Scriptable Objects/Dog")]
[ManageableData, InlineProperty]
public class DogSO : ScriptableObject
{
    [TabGroup("Stats")]
    public float Health;
    
    [TabGroup("Stats")]
    public float Aggression;
    
    [TabGroup("Stats")]
    public float Stamina;

    [TabGroup("Stats")]
    public float Satiety;

    [TabGroup("Stats")]
    public float Hydration;

    [TabGroup("Stats")]
    public float Fun;
    
    [TabGroup("Stat Manager")]
    public StatManager StatManager;
    
    [TabGroup("Conditions"), InlineEditor(Expanded = true)]
    public ScriptableBoolValue BallInHand;
    
    [TabGroup("Conditions"), InlineEditor(Expanded = true)]
    public ScriptableBoolValue FetchBall;
    
    [TabGroup("Conditions"), InlineEditor(Expanded = true)]
    public ScriptableBoolValue BallThrown;
    
    [TabGroup("Conditions"), InlineEditor(Expanded = true)]
    public ScriptableBoolValue ReturnBall;
    
    [TabGroup("Conditions"), InlineEditor(Expanded = true)]
    public ScriptableBoolValue DropBall;
    
    [TabGroup("Conditions"), InlineEditor(Expanded = true)]
    public ScriptableBoolValue BallReturned;
    
    [TabGroup("Conditions"), InlineEditor(Expanded = true)]
    public ScriptableBoolValue DogCalled;
    
    [TabGroup("Conditions"), InlineEditor(Expanded = true)]
    public ScriptableBoolValue FoodAvailable;
    
    [TabGroup("Conditions"), InlineEditor(Expanded = true)]
    public ScriptableBoolValue WaterAvailable;
    
    [TabGroup("Conditions"), InlineEditor(Expanded = true)]
    public ScriptableBoolValue RestingSpotAvailable;
    
    [TabGroup("Conditions"), InlineEditor(Expanded = true)]
    public ScriptableBoolValue ballAvailable;
    
    [FoldoutGroup("Stats Settings", expanded: false), SerializeField]
    private float defaultHealth;
    
    [FoldoutGroup("Stats Settings", expanded: false), SerializeField]
    private float defaultAggression;
    
    [FoldoutGroup("Stats Settings", expanded: false), SerializeField]
    private float defaultStamina;
    
    [FoldoutGroup("Stats Settings", expanded: false), SerializeField]
    private float defaultHunger;
    
    [FoldoutGroup("Stats Settings", expanded: false), SerializeField]
    private float defaultThirst;
    
    [FoldoutGroup("Stats Settings", expanded: false), SerializeField]
    private float defaultFun;
    
    // [TabGroup("Action Costs"), FoldoutGroup("GOAP Settings", expanded: false), InlineEditor, SerializeField]
    // private ActionCostsSO actionCosts;

    [TabGroup("Action Costs")]
    public float SleepCosts;
    
    [TabGroup("Action Costs")]
    public float RestCosts;
    
    [TabGroup("Action Costs")]
    public float SeekAttentionCosts;
    
    [TabGroup("Action Costs")]
    public float RageObstacle1Costs;
    
    [TabGroup("Action Costs")]
    public float RageObstacle2Costs;
    
    [TabGroup("Action Costs")]
    public float RageObstacle3Costs;
    
    [TabGroup("Action Costs")]
    public float DiggingCosts;
    
    [FoldoutGroup("Action Costs Settings", expanded: false), SerializeField]
    private float defaultSleepCosts;
    
    [FoldoutGroup("Action Costs Settings"), SerializeField]
    private float defaultRestCosts;
    
    [FoldoutGroup("Action Costs Settings"), SerializeField]
    private float defaultSeekAttentionCosts;
    
    [FoldoutGroup("Action Costs Settings"), SerializeField]
    private float defaultObstacle1RageCosts;
    
    [FoldoutGroup("Action Costs Settings"), SerializeField]
    private float defaultObstacle2RageCosts;
    
    [FoldoutGroup("Action Costs Settings"), SerializeField]
    private float defaultObstacle3RageCosts;
    
    [FoldoutGroup("Action Costs Settings"), SerializeField]
    private float defaultDiggingCosts;
    
    [TabGroup("Goal Priorities")]
    public float IdlePrio;
    
    [TabGroup("Goal Priorities")]
    public float WanderPrio;
    
    [TabGroup("Goal Priorities")]
    public float KeepStaminaUpPrio;
    
    [TabGroup("Goal Priorities")]
    public float KeepHydrationLevelUpPrio;
    
    [TabGroup("Goal Priorities")]
    public float KeepSatietyLevelUpPrio;
    
    [TabGroup("Goal Priorities")]
    public float KeepFunUpPrio;
    
    [TabGroup("Goal Priorities")]
    public float FetchBallAndReturnItPrio;
    
    [FoldoutGroup("Goal Priorities Settings", expanded: false), SerializeField]
    private float defaultIdlePrio;
    
    [FoldoutGroup("Goal Priorities Settings"), SerializeField]
    private float defaultWanderPrio;
    
    [FoldoutGroup("Goal Priorities Settings"), SerializeField]
    private float defaultKeepStaminaUpPrio;
    
    [FoldoutGroup("Goal Priorities Settings"), SerializeField]
    private float defaultKeepHydrationLevelUpPrio;
    
    [FoldoutGroup("Goal Priorities Settings"), SerializeField]
    private float defaultKeepSatietyLevelUpPrio;
    
    [FoldoutGroup("Goal Priorities Settings"), SerializeField]
    private float defaultKeepFunUpPrio;
    
    [FoldoutGroup("Goal Priorities Settings"), SerializeField]
    private float defaultFetchBallAndReturnItPrio;
    
    [TabGroup("Settings"), InlineEditor(Expanded = true), SerializeField]
    public DogParamsSO Settings;
    
    [HideInInspector]
    public bool SeekingAttention;

    [HideInInspector]
    public bool ballInMouth;

    private void OnEnable() {
        // Stats settings
        Health = defaultHealth;
        Aggression = defaultAggression;
        Stamina = defaultStamina;
        Satiety = defaultHunger;
        Hydration = defaultThirst;
        Fun = defaultFun;

        // Action costs settings
        SleepCosts = defaultSleepCosts;
        RestCosts = defaultRestCosts;
        SeekAttentionCosts = defaultSeekAttentionCosts;
        RageObstacle1Costs = defaultObstacle1RageCosts;
        RageObstacle2Costs = defaultObstacle2RageCosts;
        RageObstacle3Costs = defaultObstacle3RageCosts;       
        DiggingCosts = defaultDiggingCosts;
        
        // Goal priorities settings
        IdlePrio = defaultIdlePrio;
        WanderPrio = defaultWanderPrio;
        KeepStaminaUpPrio = defaultKeepStaminaUpPrio;
        KeepHydrationLevelUpPrio = defaultKeepHydrationLevelUpPrio;
        KeepSatietyLevelUpPrio = defaultKeepSatietyLevelUpPrio;
        KeepFunUpPrio = defaultKeepFunUpPrio;
        FetchBallAndReturnItPrio = defaultFetchBallAndReturnItPrio;
    }

    public void UpdateDogBehaviour() {
        UpdateStats();
        UpdateActionCosts();
        UpdatePriorities();
    }

    private void UpdateStats() {
        if (Stamina <= 0) {
            Health -= 1f;
            Aggression += 1f;
        }
        if (Satiety <= 0) {
            Health -= 1f;
            Aggression += 1f;
        }
        if (Hydration <= 0) {
            Health -= 1f;
            Aggression += 1f;
        }
        
        Health = Mathf.Clamp(Health, 0f, 100f);
        Aggression = Mathf.Clamp(Aggression, 0f, 100f);
        Satiety = Mathf.Clamp(Satiety - StatManager.SatietyLost, 0f, 100f);
        Fun = Mathf.Clamp(Fun - StatManager.FunLost, 0f, 100f);
        Stamina = Mathf.Clamp(Stamina - StatManager.StaminaLost, 0f, 100f);
        Hydration = Mathf.Clamp(Hydration - StatManager.HydrationLost, 0f, 100f);
    }
    
    private void UpdateActionCosts() {
        SleepCosts = CalculateGoodBehaviourActionCost(defaultSleepCosts, Stamina, Aggression);
        RestCosts = CalculateBadBehaviourActionCost(defaultRestCosts, Stamina, Aggression);
        SeekAttentionCosts = CalculateGoodBehaviourActionCost(defaultSeekAttentionCosts, Fun, Aggression);
        RageObstacle1Costs = CalculateBadBehaviourActionCost(defaultObstacle1RageCosts, Fun, Aggression);
        RageObstacle2Costs = CalculateBadBehaviourActionCost(defaultObstacle2RageCosts, Fun, Aggression);
        RageObstacle3Costs = CalculateBadBehaviourActionCost(defaultObstacle3RageCosts, Fun, Aggression);       
        DiggingCosts = CalculateBadBehaviourActionCost(defaultDiggingCosts, Fun, Aggression);       
    }
        
    private float CalculateGoodBehaviourActionCost(float defaultCost, float currentStatValue, float currentAggression){
        float actionCost = defaultCost - (currentStatValue / 10f) + (currentAggression / 10) + UnityEngine.Random.Range(-1f, 1f);
        return actionCost;       
    }

    private float CalculateBadBehaviourActionCost(float defaultCost, float currentStatValue, float currentAggression) {
        float actionCost = defaultCost + (currentStatValue / 10f) - (currentAggression / 10) + UnityEngine.Random.Range(-1f, 1f);
        return actionCost;  
    }
    
    private void UpdatePriorities() {
        KeepStaminaUpPrio = CalculatePriorities(defaultKeepStaminaUpPrio, Stamina);
        KeepHydrationLevelUpPrio = CalculatePriorities(defaultKeepHydrationLevelUpPrio, Satiety);
        KeepSatietyLevelUpPrio = CalculatePriorities(defaultKeepSatietyLevelUpPrio, Hydration);
        KeepFunUpPrio = CalculatePriorities(defaultKeepFunUpPrio, Fun);
        FetchBallAndReturnItPrio = CalculatePriorities(defaultFetchBallAndReturnItPrio, Fun);
    }
        
    private float CalculatePriorities(float defaultPriority, float currentStatValue) {
        float goalPriority = defaultPriority - (currentStatValue / 10f);
        return goalPriority;
    }
}