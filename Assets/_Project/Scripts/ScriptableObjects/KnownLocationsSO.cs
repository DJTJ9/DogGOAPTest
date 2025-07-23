using UnityEngine;
using Sirenix.OdinInspector;

[ManageableData, InlineEditor(Expanded = true)]
[CreateAssetMenu(fileName = "Known Locations", menuName = "Scriptable Objects/Known Locations")]
public class KnownLocationsSO : ScriptableObject
{
    [FoldoutGroup("Known Locations", expanded: true), SerializeField]
    public Transform RestingPosition;

    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform FoodBowl;

    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform WaterBowl;

    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform Obstacle1;
    
    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform Obstacle2;
    
    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform Obstacle3;

    [FoldoutGroup("Known Locations"), SerializeField]
    public Transform PlayerTransform;

    [FoldoutGroup("Known Locations"), SerializeField]
    public GameObject Ball;
}