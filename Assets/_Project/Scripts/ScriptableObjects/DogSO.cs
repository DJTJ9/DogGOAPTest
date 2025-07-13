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
    
    [TabGroup("Conditions")]
    public bool BallInHand;
    
    [TabGroup("Conditions")]
    public bool BallThrown;
    
    [TabGroup("Conditions")]
    public bool ReturnBall;
    
    [TabGroup("Conditions")]
    public bool BallReturned;
    
    [TabGroup("Conditions")]
    public bool DogCalled;
    
    [TabGroup("Conditions"), InlineEditor(Expanded = false)]
    public ScriptableBoolValue FoodAvailable;
    
    [TabGroup("Conditions"), InlineEditor(Expanded = false)]
    public ScriptableBoolValue WaterAvailable;
    
    [TabGroup("Conditions"), InlineEditor(Expanded = false)]
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
        BallInHand = defaultBallInHand;
        BallThrown = defaultBallThrown;
        ReturnBall = defaultReturnBall;
        BallReturned = defaultBallReturned;
        DogCalled = defaultDogCalled;
        FoodAvailable.Value = defaultFoodAvailable;
        WaterAvailable.Value = defaultWaterAvailable;
        RestingSpotAvailable.Value = defaultRestingSpotAvailable;
    }
}