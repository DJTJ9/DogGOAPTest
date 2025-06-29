using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Action", menuName = "GOAP/Action")]
public class ActionScriptableObject : ScriptableObject
{
    [SerializeField] private string actionName;
    [SerializeField] private StrategyScriptableObject strategy;
    [SerializeField, Range(0, 10)] private int cost = 1;

    [Header("Für Move-Strategy")]
    [SerializeField] private TargetType targetType;
    [SerializeField] private string targetLocationName;

    // Hier werden durch den Editor die Namen der Preconditions und Effects eingetragen
    [SerializeField] private List<string> preconditionNames = new List<string>();
    [SerializeField] private List<string> effectNames = new List<string>();

    public enum TargetType
    {
        None,
        RestingPosition,
        FoodBowl,
        WaterBowl,
        DoorOne,
        DoorTwo,
        Custom
    }

    public AgentAction CreateAction(GoapAgent agent, Dictionary<string, AgentBelief> beliefs)
    {
        var builder = new AgentAction.Builder(actionName)
            .WithCost(cost);

        // Strategy initialisieren
        IActionStrategy actionStrategy = null;

        if (strategy is MoveStrategyScriptableObject moveStrategyObj)
        {
            // Bei Move-Strategy brauchen wir eine spezielle Behandlung für das Ziel
            switch (targetType)
            {
                case TargetType.RestingPosition:
                    actionStrategy = moveStrategyObj.CreateStrategy(agent, () => agent.GetRestingPosition().position);
                    break;
                case TargetType.FoodBowl:
                    actionStrategy = moveStrategyObj.CreateStrategy(agent, () => agent.GetFoodBowl().position);
                    break;
                case TargetType.WaterBowl:
                    actionStrategy = moveStrategyObj.CreateStrategy(agent, () => agent.GetWaterBowl().position);
                    break;
                case TargetType.DoorOne:
                    actionStrategy = moveStrategyObj.CreateStrategy(agent, () => agent.GetDoorOnePosition().position);
                    break;
                case TargetType.DoorTwo:
                    actionStrategy = moveStrategyObj.CreateStrategy(agent, () => agent.GetDoorTwoPosition().position);
                    break;
                // case TargetType.Custom:
                //     if (beliefs.ContainsKey(targetLocationName) && beliefs[targetLocationName] is LocationBelief locBelief)
                //     {
                //         actionStrategy = moveStrategyObj.CreateStrategy(agent, () => locBelief.Location);
                //     }
                //     break;
            }
        }
        else
        {
            // Für alle anderen Strategies einfach die Standardmethode verwenden
            actionStrategy = strategy.CreateStrategy(agent);
        }

        if (actionStrategy != null)
        {
            builder.WithStrategy(actionStrategy);
        }
        else
        {
            Debug.LogError($"Konnte keine Strategy für Aktion {actionName} erstellen!");
        }

        // Preconditions und Effects hinzufügen
        foreach (var preconditionName in preconditionNames)
        {
            if (beliefs.ContainsKey(preconditionName))
            {
                builder.AddPrecondition(beliefs[preconditionName]);
            }
            else
            {
                Debug.LogWarning($"Belief {preconditionName} nicht gefunden für Precondition von {actionName}");
            }
        }

        foreach (var effectName in effectNames)
        {
            if (beliefs.ContainsKey(effectName))
            {
                builder.AddEffect(beliefs[effectName]);
            }
            else
            {
                Debug.LogWarning($"Belief {effectName} nicht gefunden für Effect von {actionName}");
            }
        }

        return builder.Build();
    }
}
