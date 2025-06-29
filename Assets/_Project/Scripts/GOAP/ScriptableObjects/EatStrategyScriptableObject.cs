using UnityEngine;

[CreateAssetMenu(fileName = "Eat Strategy", menuName = "GOAP/Strategies/Eat Strategy")]
public class EatStrategyScriptableObject : StrategyScriptableObject
{
    public override IActionStrategy CreateStrategy(GoapAgent agent)
    {
        return new EatAndWaitStrategy(agent.GetComponent<AnimationController>());
    }
}