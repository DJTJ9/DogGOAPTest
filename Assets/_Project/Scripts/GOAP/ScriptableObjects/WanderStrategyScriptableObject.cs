using UnityEngine;

[CreateAssetMenu(fileName = "Wander Strategy", menuName = "GOAP/Strategies/Wander Strategy")]
public class WanderStrategyScriptableObject : StrategyScriptableObject
{
    [SerializeField] private float wanderRadius = 20f;
    [SerializeField] private int wanderSteps = 5;

    public override IActionStrategy CreateStrategy(GoapAgent agent)
    {
        return new WanderStrategy(agent.GetComponent<UnityEngine.AI.NavMeshAgent>(), wanderRadius, wanderSteps);
    }
}
