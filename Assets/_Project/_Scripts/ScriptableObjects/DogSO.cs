using ScriptableValues;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Dog", menuName = "Scriptable Objects/Dog")]
[ManageableData, InlineProperty]
public class DogSO : ScriptableObject
{
    [TabGroup("Stats")]
    public float Stamina;

    [TabGroup("Stats")]
    public float Hunger;

    [TabGroup("Stats")]
    public float Thirst;

    [TabGroup("Stats")]
    public float Boredom;
    
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
    
    [TabGroup("Action Costs"), FoldoutGroup("GOAP Settings"), InlineEditor, SerializeField]
    private ActionCostsSO actionCosts;
    
    [TabGroup("Goal Prios"), FoldoutGroup("GOAP Settings"), InlineEditor, SerializeField]
    private GoalPrioritiesSO goalPriorities;
    
    [TabGroup("Params"), FoldoutGroup("GOAP Settings", expanded: false), InlineEditor] [SerializeField]
    private DogParamsSO dogParams;
}