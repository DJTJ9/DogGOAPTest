using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace ScriptableValues
{
    public class ValueAsset<T> : ScriptableObject
    {
        [SerializeField] protected T currentValue;
        [FoldoutGroup("Settings"), SerializeField] protected T defaultValue;

        // Event für Änderungen
        public event Action<T> OnValueChanged;

        // Property mit Getter und Setter
        public virtual T Value
        {
            get => currentValue;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(currentValue, value))
                {
                    currentValue = value;
                    OnValueChanged?.Invoke(currentValue);
                }
            }
        }

        // Wird beim Spielstart aufgerufen
        protected virtual void OnEnable()
        {
            currentValue = defaultValue;
        }

        // Methode zum Zurücksetzen
        public virtual void ResetToDefault()
        {
            Value = defaultValue;
        }
    }
}