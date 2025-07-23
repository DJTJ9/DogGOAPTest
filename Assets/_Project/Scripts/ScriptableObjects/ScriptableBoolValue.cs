using UnityEngine;
using System;

namespace ScriptableValues
{
    [CreateAssetMenu(fileName = "Bool Value", menuName = "Scriptable Values/Bool")]
    public class ScriptableBoolValue : ScriptableObject
    {
        [SerializeField] private bool currentValue;
        [SerializeField] private bool defaultValue;

        // Event für Änderungen
        public event Action<bool> OnValueChanged;

        // Property mit Getter und Setter
        public bool Value
        {
            get => currentValue;
            set
            {
                if (currentValue != value)
                {
                    currentValue = value;
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
        public void Toggle()
        {
            Value = !currentValue;
        }
        
        public static implicit operator bool(ScriptableBoolValue valueRef) {
            return valueRef.Value;
        }
    }
}
