using System;
using System.Collections.Generic;
using UnityEngine;

public class BeliefFactory {
    readonly GoapAgent agent;
    readonly Dictionary<Beliefs, AgentBelief> beliefs;

    public BeliefFactory(GoapAgent agent, Dictionary<Beliefs, AgentBelief> beliefs) {
        this.agent = agent;
        this.beliefs = beliefs;
    }

    public void AddBelief(Beliefs type, Func<bool> condition) {
        beliefs.Add(type, new AgentBelief.Builder(type)
            .WithCondition(condition)
            .Build());
    }

    public void AddSensorBelief(Beliefs type, Sensor sensor) {
        beliefs.Add(type, new AgentBelief.Builder(type)
            .WithCondition(() => sensor.IsTargetInRange)
            .WithLocation(() => sensor.TargetPosition)
            .Build());
    }

    public void AddLocationBelief(Beliefs type, float distance, Transform locationCondition) {
        AddLocationBelief(type, distance, locationCondition.position);
    }

    public void AddLocationBelief(Beliefs type, float distance, Vector3 locationCondition) {
        beliefs.Add(type, new AgentBelief.Builder(type)
            .WithCondition(() => InRangeOf(locationCondition, distance))
            .WithLocation(() => locationCondition)
            .Build());
    }
    
    bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(agent.transform.position, pos) < range;
}

    public class AgentBelief {
    public Beliefs Type { get; }

    Func<bool> condition = () => false;
    Func<Vector3> observedLocation = () => Vector3.zero;

    public Vector3 Location => observedLocation();

    AgentBelief(Beliefs type) {
        Type = type;
    }

    public bool Evaluate() => condition();

    public class Builder {
        readonly AgentBelief belief;

        public Builder(Beliefs type) {
            belief = new AgentBelief(type);
        }
        
        public Builder WithCondition(Func<bool> condition) {
            belief.condition = condition;
            return this;
        }
        
        public Builder WithLocation(Func<Vector3> observedLocation) {
            belief.observedLocation = observedLocation;
            return this;
        }
        
        public AgentBelief Build() {
            return belief;
        }
    }
}