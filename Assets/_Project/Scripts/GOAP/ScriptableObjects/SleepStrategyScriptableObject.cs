using UnityEngine;

[CreateAssetMenu(fileName = "New Sleep Strategy", menuName = "GOAP/Strategies/Sleep Strategy")]
public class SleepStrategyScriptableObject : StrategyScriptableObject
{
    [SerializeField] private float sleepDuration = 10f;

    public override IActionStrategy CreateStrategy(GoapAgent agent)
    {
        return new SleepAndWaitStrategy(agent.GetComponent<AnimationController>());
    }
}
