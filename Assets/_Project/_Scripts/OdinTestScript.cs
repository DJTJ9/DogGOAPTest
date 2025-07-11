using System;
using ScriptableValues;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


public class OdinTestScript : MonoBehaviour
{
    [OnInspectorGUI("AdjustColor")]
    public ColorReference color = new ColorReference();

    private Graphic graphic;

    [OnInspectorGUI("AdjustFloatValue")]
    public FloatReference floatValue = new FloatReference();

    private Graphic floatGraphic;
    private float maxHealth;

    private void OnEnable() {
        AdjustColor();
    }

    private void AdjustColor() {
        if (graphic == null)
            graphic = GetComponent<Graphic>();

        if (graphic != null)
            graphic.color = this.color;
    }

    private void AdjustFloatValue() {
        if (maxHealth == 0)
            maxHealth = floatValue;
    }
}