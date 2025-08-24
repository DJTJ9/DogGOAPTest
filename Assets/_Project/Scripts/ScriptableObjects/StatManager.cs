using Sirenix.OdinInspector;
using UnityEngine;

[InlineEditor(Expanded = true)]
[CreateAssetMenu(fileName = "StatManager", menuName = "Scriptable Objects/StatManager")]
public class StatManager : ScriptableObject
{
    [FoldoutGroup("Stat Changes", expanded: true)]
    public float StaminaLost;
    
    [FoldoutGroup("Stat Changes")]
    public float SatietyLost;
    
    [FoldoutGroup("Stat Changes")]
    public float HydrationLost;
    
    [FoldoutGroup("Stat Changes")]
    public float FunLost;

    [FoldoutGroup("Stat Changes")]
    public float HealthLost;

    [FoldoutGroup("Stat Changes")]
    public float AggressionGain;
}