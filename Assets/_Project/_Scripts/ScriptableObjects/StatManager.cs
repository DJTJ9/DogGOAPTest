using Sirenix.OdinInspector;
using UnityEngine;

[ManageableData, InlineEditor(Expanded = true)]
[CreateAssetMenu(fileName = "StatManager", menuName = "ScriptableObjects/StatManager")]
public class StatManager : ScriptableObject
{
    [FoldoutGroup("Stat Loses", expanded: true)]
    public float StaminaLost;
    [FoldoutGroup("Stat Loses")]
    public float HungerLost;
    [FoldoutGroup("Stat Loses")]
    public float ThirstLost;
    [FoldoutGroup("Stat Loses")]
    public float BoredomLost;
}