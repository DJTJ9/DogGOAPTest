using UnityEngine;
using System;

namespace ScriptableValues
{
    [CreateAssetMenu(fileName = "String Value", menuName = "ScriptableValues/String")]
    public class ScriptableStringValue : ScriptableObject
    {
        [SerializeField] private string currentValue;
        [SerializeField] private string defaultValue;

        // Event für Änderungen
        public event Action<string> OnValueChanged;

        // Property mit Getter und Setter
        public string Value
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
        public void Append(string text)
        {
            Value += text;
        }

        public void Clear()
        {
            Value = string.Empty;
        }

        public bool Contains(string text)
        {
            return currentValue.Contains(text);
        }
    }
}
