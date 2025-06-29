using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Move Strategy", menuName = "GOAP/Strategies/Move Strategy")]
public class MoveStrategyScriptableObject : StrategyScriptableObject
{
    [SerializeField] private float stoppingDistance = 1f;

    public override IActionStrategy CreateStrategy(GoapAgent agent)
    {
        // Hier müssen wir eine Möglichkeit schaffen, das Ziel zu definieren
        // Das wird später beim Erstellen der Aktion festgelegt
        return null;
    }

    public IActionStrategy CreateStrategy(GoapAgent agent, Func<Vector3> destination)
    {
        return new MoveStrategy(agent.GetComponent<UnityEngine.AI.NavMeshAgent>(), destination, stoppingDistance);
    }
}
