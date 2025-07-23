using UnityEngine;
using System;

namespace ScriptableValues
{
    [CreateAssetMenu(fileName = "Vector3 Value", menuName = "Scriptable Values/Vector3")]
    public class ScriptableVector3Value : ScriptableObject
    {
        [SerializeField] private Vector3 currentValue;
        [SerializeField] private Vector3 defaultValue;

        // Event für Änderungen
        public event Action<Vector3> OnValueChanged;

        // Property mit Getter und Setter
        public Vector3 Value
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
        public void Add(Vector3 vector)
        {
            Value += vector;
        }

        public void Scale(float factor)
        {
            Value *= factor;
        }

        public float Magnitude()
        {
            return currentValue.magnitude;
        }
    }
}
