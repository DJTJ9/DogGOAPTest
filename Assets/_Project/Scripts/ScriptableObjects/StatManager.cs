using Sirenix.OdinInspector;
using UnityEngine;

[ManageableData, InlineEditor(Expanded = true)]
[CreateAssetMenu(fileName = "StatManager", menuName = "Scriptable Objects/StatManager")]
public class StatManager : ScriptableObject
{
    [FoldoutGroup("Stat Loses", expanded: true)]
    public float StaminaLost;
    
    [FoldoutGroup("Stat Loses")]
    public float SatietyLost;
    
    [FoldoutGroup("Stat Loses")]
    public float HydrationLost;
    
    [FoldoutGroup("Stat Loses")]
    public float FunLost;
}