using UnityEngine;
using Sirenix.OdinInspector;

[ManageableData, InlineEditor(Expanded = true)]
[CreateAssetMenu(fileName = "Known Locations", menuName = "Scriptable Objects/Known Locations")]
public class KnownLocationsSO : ScriptableObject
{
    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform PlayerTransform;

    [FoldoutGroup("Known Locations"), SerializeField]
    public GameObject Ball;

    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform FoodBowl1;
    
    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform FoodBowl2;

    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform WaterBowl1;
    
    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform WaterBowl2;

    [FoldoutGroup("Known Locations", expanded: true), SerializeField]
    public Transform RestingPosition;
    
    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform Obstacle1;
    
    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform Obstacle2;
    
    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform Obstacle3;
    
    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform Obstacle4;
}