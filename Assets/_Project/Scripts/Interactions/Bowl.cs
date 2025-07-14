using System;
using ScriptableValues;
using UnityEngine;

public class Bowl : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Transform fillAmount;
    [SerializeField]
    private float minYPosition = -0.08f;
    [SerializeField]
    private float maxYPosition = 0f;

    public void Interact() {
        if (fillAmount.position.y >= maxYPosition) return;
        Vector3 newPosition = fillAmount.position + new Vector3(0f, 0.01f, 0f);
        newPosition.y = Mathf.Clamp(newPosition.y, minYPosition, maxYPosition);
        fillAmount.position = newPosition;
        Debug.Log("Food or Water refilled");
    }
}