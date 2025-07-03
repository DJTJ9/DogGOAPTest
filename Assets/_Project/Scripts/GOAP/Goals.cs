using System.Collections.Generic;

public enum GoalType
{
    Idle,
    Wander,
    KeepThirstLevelUp,
    KeepHungerLevelUp,
    KeepStaminaUp,
    AttackPlayer,
    KeepBoredomLow,
    GetAttention,
    FetchBallAndReturnIt
}

public class AgentGoal
{
    public GoalType Type { get; }
    public float Priority { get; private set; }
    public HashSet<AgentBelief> DesiredEffects { get; } = new();


    
    
    AgentGoal(GoalType type) {
        Type = type;
    }

    public class Builder
    {
        readonly AgentGoal goal;

        public Builder(GoalType name) {
            goal = new AgentGoal(name);
        }

        public Builder WithPriority(float priority) {
            goal.Priority = priority;
            return this;
        }

        public Builder WithDesiredEffect(AgentBelief effect) {
            goal.DesiredEffects.Add(effect);
            return this;
        }

        public AgentGoal Build() {
            return goal;
        }
    }
}