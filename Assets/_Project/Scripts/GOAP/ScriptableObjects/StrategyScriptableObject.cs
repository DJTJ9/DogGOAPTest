using UnityEngine;

public abstract class StrategyScriptableObject : ScriptableObject
{
    [SerializeField] private string strategyName;

    public string StrategyName => strategyName;

    public abstract IActionStrategy CreateStrategy(GoapAgent agent);
}
