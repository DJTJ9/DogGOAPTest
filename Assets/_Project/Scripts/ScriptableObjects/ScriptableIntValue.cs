using UnityEngine;
using System;

namespace ScriptableValues
{
    [CreateAssetMenu(fileName = "Int Value", menuName = "ScriptableValues/Int")]
    public class ScriptableIntValue : ScriptableObject
    {
        [SerializeField] private int currentValue;
        [SerializeField] private int defaultValue;
        [SerializeField] private int minValue = int.MinValue;
        [SerializeField] private int maxValue = int.MaxValue;

        // Event für Änderungen
        public event Action<int> OnValueChanged;

        // Property mit Getter und Setter
        public int Value
        {
            get => currentValue;
            set
            {
                int clampedValue = Mathf.Clamp(value, minValue, maxValue);
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
        public void Add(int amount)
        {
            Value += amount;
        }

        public void Subtract(int amount)
        {
            Value -= amount;
        }

        public void Multiply(int factor)
        {
            Value *= factor;
        }
    }
}
