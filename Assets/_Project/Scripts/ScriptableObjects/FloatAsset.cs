using UnityEngine;
using Sirenix.OdinInspector;

namespace ScriptableValues
{
    [CreateAssetMenu(fileName = "FloatAsset", menuName = "Value Assets/Float")]   
    public class FloatAsset : ValueAsset<float>
    {
        [FoldoutGroup("Settings"), SerializeField] private float minValue = float.MinValue;
        [FoldoutGroup("Settings"), SerializeField] private float maxValue = float.MaxValue;

        // Öffentliche Properties für den Zugriff im Editor
        public float MinValue => minValue;
        public float MaxValue => maxValue;
        public float DefaultValue => defaultValue;

        // Property mit Getter und Setter (überschrieben für Clamp-Funktionalität)
        public override float Value
        {
            get => currentValue;
            set
            {
                float clampedValue = Mathf.Clamp(value, minValue, maxValue);
                if (currentValue != clampedValue)
                {
                    currentValue = clampedValue;
                    // OnValueChanged?.Invoke(currentValue);
                }
            }
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