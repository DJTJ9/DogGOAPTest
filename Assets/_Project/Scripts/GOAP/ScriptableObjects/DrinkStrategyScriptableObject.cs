using UnityEngine;

[CreateAssetMenu(fileName = "Drink Strategy", menuName = "GOAP/Strategies/Drink Strategy")]
public class DrinkStrategyScriptableObject : StrategyScriptableObject
{
    public override IActionStrategy CreateStrategy(GoapAgent agent)
    {
        return new DrinkAndWaitStrategy(agent.GetComponent<AnimationController>());
    }
}
