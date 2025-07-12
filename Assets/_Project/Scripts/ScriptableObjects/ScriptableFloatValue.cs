using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace ScriptableValues
{
    [CreateAssetMenu(fileName = "Float Value", menuName = "ScriptableValues/Float")]
    public class ScriptableFloatValue : ScriptableObject
    {
        [SerializeField, ProgressBar(0, 100)] private float currentValue;
        [FoldoutGroup("Settings", expanded: false), SerializeField] private float defaultValue;
        [FoldoutGroup("Settings"), SerializeField] private float minValue;
        [FoldoutGroup("Settings"), SerializeField] private float maxValue;

        // Öffentliche Properties für den Zugriff im Editor
        public float MinValue => 0;
        public float MaxValue => 100;
        public float DefaultValue => defaultValue;

        // Event für Änderungen
        public event Action<float> OnValueChanged;

        // Property mit Getter und Setter
        public float Value
        {
            get => currentValue;
            set
            {
                float clampedValue = Mathf.Clamp(value, minValue, maxValue);
                if (currentValue != clampedValue)
                {
                    currentValue = clampedValue;
                    OnValueChanged?.Invoke(currentValue);
                }
            }
        }

        // Wird beim Spielstart aufgerufen
        private void OnEnable()
        {
            currentValue = defaultValue;
        }

        // Methode zum Zurücksetzen
        public void ResetToDefault()
        {
            Value = defaultValue;
        }

        // Nützliche Hilfsmethoden
        public void Add(float amount)
        {
            Value += amount;
        }

        public void Subtract(float amount)
        {
            Value -= amount;
        }

        public void Multiply(float factor)
        {
            Value *= factor;
        }
    }
}
