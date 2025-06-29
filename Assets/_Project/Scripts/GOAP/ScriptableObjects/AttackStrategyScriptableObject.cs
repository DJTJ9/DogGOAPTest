using UnityEngine;

[CreateAssetMenu(fileName = "Attack Strategy", menuName = "GOAP/Strategies/Attack Strategy")]
public class AttackStrategyScriptableObject : StrategyScriptableObject
{
    public override IActionStrategy CreateStrategy(GoapAgent agent)
    {
        return new AttackStrategy(agent.GetComponent<AnimationController>());
    }
}
