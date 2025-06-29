using UnityEngine;

namespace ScriptableValues
{
    /// <summary>
    /// Diese Klasse dient nur als Dokumentation für die Verwendung der ScriptableValues.
    /// </summary>
    public class ScriptableValueReadme : MonoBehaviour
    {
        /* Beispiel für die Verwendung von ScriptableValues:
         * 
         * 1. Erstellen eines ScriptableValue Assets:
         *    - Rechtsklick im Project-Fenster
         *    - Wähle: Create > ScriptableValues > [Gewünschter Typ]
         * 
         * 2. Referenzierung in einer Klasse:
         *    [SerializeField] private ScriptableFloatValue myFloatValue;
         * 
         * 3. Auf Wertänderungen reagieren:
         *    void OnEnable() {
         *        myFloatValue.OnValueChanged += HandleValueChanged;
         *    }
         *    
         *    void OnDisable() {
         *        myFloatValue.OnValueChanged -= HandleValueChanged;
         *    }
         *    
         *    void HandleValueChanged(float newValue) {
         *        Debug.Log($"Wert hat sich geändert zu: {newValue}");
         *    }
         * 
         * 4. Werte setzen und auslesen:
         *    myFloatValue.Value = 42f;            // Setzen eines Werts
         *    float currentValue = myFloatValue.Value;  // Auslesen eines Werts
         *    
         * 5. Hilfsmethoden verwenden:
         *    myFloatValue.Add(10f);              // Addiert 10 zum aktuellen Wert
         *    myFloatValue.ResetToDefault();      // Setzt auf Standardwert zurück
         */
    }
}
