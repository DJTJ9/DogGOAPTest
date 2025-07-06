    void UpdateStats() {
        actionCosts.Sleep = Mathf.Clamp(Random.Range(actionCosts.minSleepCosts, actionCosts.maxSleepCosts), actionCosts.minSleepCosts, actionCosts.maxSleepCosts);
        actionCosts.Rest = Mathf.Clamp(Random.Range(actionCosts.minRestCosts, actionCosts.maxRestCosts), actionCosts.minRestCosts, actionCosts.maxRestCosts);
        actionCosts.Attention = Mathf.Clamp(Random.Range(actionCosts.minAttentionCosts, actionCosts.maxAttentionCosts), actionCosts.minAttentionCosts, actionCosts.maxAttentionCosts);
        actionCosts.Rage = Mathf.Clamp(Random.Range(actionCosts.minRageCosts, actionCosts.maxRageCosts), actionCosts.minRageCosts, actionCosts.maxRageCosts);
