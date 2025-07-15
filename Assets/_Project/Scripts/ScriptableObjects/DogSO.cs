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
    private float defaultBoredom;
    
    [FoldoutGroup("Conditions Settings", expanded: false), SerializeField]
    private bool defaultBallInHand;
    
    [FoldoutGroup("Conditions Settings", expanded: false), SerializeField]
    private bool defaultBallThrown;
    
    [FoldoutGroup("Conditions Settings", expanded: false), SerializeField]
    private bool defaultReturnBall;
    
    [FoldoutGroup("Conditions Settings", expanded: false), SerializeField]
    private bool defaultDropBall;
    
    [FoldoutGroup("Conditions Settings", expanded: false), SerializeField]
    private bool defaultBallReturned;
    
    [FoldoutGroup("Conditions Settings", expanded: false), SerializeField]
    private bool defaultDogCalled;
    
    [FoldoutGroup("Conditions Settings", expanded: false), SerializeField]
    private bool defaultFoodAvailable;
    
    [FoldoutGroup("Conditions Settings", expanded: false), SerializeField]
    private bool defaultWaterAvailable;
    
    [FoldoutGroup("Conditions Settings", expanded: false), SerializeField]
    private bool defaultRestingSpotAvailable;
    
    [TabGroup("Action Costs"), FoldoutGroup("GOAP Settings", expanded: false), InlineEditor, SerializeField]
    private ActionCostsSO actionCosts;
    
    [TabGroup("Goal Prios"), FoldoutGroup("GOAP Settings", expanded: false), InlineEditor, SerializeField]
    private GoalPrioritiesSO goalPriorities;
    
    [TabGroup("Params"), FoldoutGroup("GOAP Settings", expanded: false), InlineEditor] [SerializeField]
    private DogParamsSO dogParams;
    
    

    private void OnEnable() {
        Health = defaultHealth;
        Aggression = defaultAggression;
        Stamina = defaultStamina;
        Satiety = defaultHunger;
        Hydration = defaultThirst;
        Fun = defaultBoredom;
        BallInHand.Value = defaultBallInHand;
        BallThrown.Value = defaultBallThrown;
        ReturnBall.Value = defaultReturnBall;
        DropBall.Value = defaultDropBall;
        BallReturned.Value = defaultBallReturned;
        DogCalled.Value = defaultDogCalled;
        FoodAvailable.Value = defaultFoodAvailable;
        WaterAvailable.Value = defaultWaterAvailable;
        RestingSpotAvailable.Value = defaultRestingSpotAvailable;
    }
}