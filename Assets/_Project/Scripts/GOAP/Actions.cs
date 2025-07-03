using System.Collections.Generic;

public enum ActionType
{
    Relax,
    WanderAround,
    MoveFromRelaxToRestArea,
    FetchBall,
    MoveFromWanderToRestArea,
    MoveToEatingPosition,
    Eat,
    MoveFromFoodBowlToRestArea,
    MoveToDrinkingPosition,
    Drink,
    MoveFromWaterBowlToRestArea,
    MoveToDoorOne,
    MoveToDoorTwo,
    MoveFromDoorOneToRestArea,
    MoveFromDoorTwoToRestArea,
    Sleep,
    Rest,
    ChasePlayer,
    AttackPlayer,
    MoveFromRelaxToRageVictim,
    AttackRageVictim,
    SeekAttention,
    PickUpBall,
    DropBall,
    MoveToPlayer,
    MoveToBall,
    ReturnToPlayer,
}

public class AgentAction {
    // public string Name { get; }
    public ActionType Type { get; }
    public float  Cost { get; private set; }
    
    public HashSet<AgentBelief> Preconditions { get; } = new();
    public HashSet<AgentBelief> Effects       { get; } = new();
    
    IActionStrategy strategy;
    public bool Complete => strategy.Complete;
    
    AgentAction(ActionType type) {
        Type = type;
    }
    
    public void Start() => strategy.Start();

    public void Update(float deltaTime) {
        // Check if the action can be performed and update the strategy
        if (strategy.CanPerform) {
            strategy.Update(deltaTime);
        }
        
        // Bail out if the strategy is still executing
        if (!strategy.Complete) return;
        
        // Apply effects
        foreach (var effect in Effects) {
            effect.Evaluate();
        }
    }
    
    public void Stop() => strategy.Stop();

    public class Builder {
        readonly AgentAction action;
        
        public Builder(ActionType type) {
            action = new AgentAction(type) {
                Cost = 1
            };
        }
        
        public Builder WithCost(float cost) {
            action.Cost = cost;
            return this;
        }
        
        public Builder WithStrategy(IActionStrategy strategy) {
            action.strategy = strategy;
            return this;
        }
        
        public Builder AddPrecondition(AgentBelief precondition) {
            action.Preconditions.Add(precondition);
            return this;
        }
        
        public Builder AddEffect(AgentBelief effect) {
            action.Effects.Add(effect);
            return this;
        }
        
        public AgentAction Build() {
            return action;
        }
    }
}