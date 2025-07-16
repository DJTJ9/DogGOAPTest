using System;
using ScriptableValues;
using UnityEngine;

public class Bowl : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Transform fillAmount;
    [SerializeField]
    private float refillAmount = 0.02f;
    [SerializeField]
    private float minYPosition = -0.08f;
    [SerializeField]
    private float maxYPosition = 0f;
    
    [SerializeField]
    private ScriptableBoolValue foodAvailable;

    private void Update() {
        foodAvailable.Value = !(fillAmount.position.y <= minYPosition);
    }

    public void Interact() {
        if (fillAmount.position.y >= maxYPosition) return;
        Vector3 newPosition = fillAmount.position + new Vector3(0f, refillAmount, 0f);
        newPosition.y = Mathf.Clamp(newPosition.y, minYPosition, maxYPosition);
        fillAmount.position = newPosition;
        Debug.Log("Food or Water refilled");
    }
}