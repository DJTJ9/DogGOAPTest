using UnityEngine;

[CreateAssetMenu(fileName = "Idle Strategy", menuName = "GOAP/Strategies/Idle Strategy")]
public class IdleStrategyScriptableObject : StrategyScriptableObject
{
    [SerializeField] private float duration = 5f;

    public override IActionStrategy CreateStrategy(GoapAgent agent)
    {
        return new IdleStrategy(duration);
    }
}
