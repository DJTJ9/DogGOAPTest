using UnityEngine;
using Sirenix.OdinInspector;

[ManageableData, InlineEditor(Expanded = true)]
[CreateAssetMenu(fileName = "Transforms", menuName = "Scriptable Objects/Transforms")]
public class TransformsSO : ScriptableObject
{
    [FoldoutGroup("Known Locations", expanded: true)]
    public Transform PlayerTransform;

    [FoldoutGroup("Known Locations")]
    public GameObject Ball;

    [FoldoutGroup("Known Locations")]
    public Transform FoodBowl1;
    
    [FoldoutGroup("Known Locations")]
    public Transform FoodBowl2;

    [FoldoutGroup("Known Locations")]
    public Transform WaterBowl1;
    
    [FoldoutGroup("Known Locations")]
    public Transform WaterBowl2;

    [FoldoutGroup("Known Locations")]
    public Transform RestingPosition;
    
    [FoldoutGroup("Known Locations")]
    public Transform Obstacle1;
    
    [FoldoutGroup("Known Locations"),]
    public Transform Obstacle2;
    
    [FoldoutGroup("Known Locations")]
    public Transform Obstacle3;
    
    [FoldoutGroup("Known Locations")]
    public Transform Obstacle4;
}
