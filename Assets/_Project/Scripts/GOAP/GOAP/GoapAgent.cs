    void OnEnable() => chaseSensor.OnTargetChanged += HandleTargetChanged;

    void OnDisable() => chaseSensor.OnTargetChanged -= HandleTargetChanged;
