using Sirenix.OdinInspector;
using UnityEngine;

[ManageableData, InlineEditor(Expanded = true)]
[CreateAssetMenu(fileName = "StatManager", menuName = "ScriptableObjects/StatManager")]
public class StatManager : ScriptableObject
{
    [FoldoutGroup("Stat Loses", expanded: true)]
    public float StaminaLevelLost;
    [FoldoutGroup("Stat Loses")]
    public float HungerLevelLost;
    [FoldoutGroup("Stat Loses")]
    public float ThirstLevelLost;
    [FoldoutGroup("Stat Loses")]
    public float BoredomLevelLost;
}