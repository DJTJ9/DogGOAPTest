using System;
using ScriptableValues;
using UnityEngine;
using UnityEngine.UI;

public class Bowl : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Transform fillAmount;
    [SerializeField]
    private float refillAmount = 0.02f;
    [SerializeField]
    private Slider refillSlider;
    [SerializeField]
    private float minYPosition = -0.08f;
    [SerializeField]
    private float maxYPosition = 0f;
    
    [SerializeField]
    private ScriptableBoolValue foodOrWaterAvailable;
    
    [SerializeField]
    private string interactionName;

    public string GetInteractionName() {
        return interactionName;
    }

    private void Update() {
        foodOrWaterAvailable.Value = !(fillAmount.position.y <= minYPosition);
    }

    public void Interact() {
        if (fillAmount.position.y >= maxYPosition) return;
        Vector3 newPosition = fillAmount.position + new Vector3(0f, refillAmount, 0f);
        newPosition.y = Mathf.Clamp(newPosition.y, minYPosition, maxYPosition);
        fillAmount.position = newPosition;
        refillSlider.value -= 25f;
        Debug.Log("Food or Water refilled");
    }
}