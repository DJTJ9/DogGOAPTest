using UnityEngine;
using System;

namespace ScriptableValues
{
    [CreateAssetMenu(fileName = "Color Value", menuName = "ScriptableValues/Color")]
    public class ScriptableColorValue : ScriptableObject
    {
        [SerializeField] private Color currentValue;
        [SerializeField] private Color defaultValue;

        // Event für Änderungen
        public event Action<Color> OnValueChanged;

        // Property mit Getter und Setter
        public Color Value
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
        public void SetAlpha(float alpha)
        {
            Color newColor = currentValue;
            newColor.a = Mathf.Clamp01(alpha);
            Value = newColor;
        }

        public Color GetWithAlpha(float alpha)
        {
            Color result = currentValue;
            result.a = Mathf.Clamp01(alpha);
            return result;
        }
    }
}
