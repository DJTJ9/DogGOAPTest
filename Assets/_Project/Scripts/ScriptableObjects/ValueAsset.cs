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

        public event Action<T> OnValueChanged;

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

        protected virtual void OnEnable()
        {
            currentValue = defaultValue;
        }

        public virtual void ResetToDefault()
        {
            Value = defaultValue;
        }
    }
}