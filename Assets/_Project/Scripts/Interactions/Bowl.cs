using System;
using UnityEngine;

public class Bowl : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Transform foodAmount;

    public void Interact() {
        foodAmount.position += new Vector3(0f, 0.01f, 0f);
        Debug.Log("Food or Water refilled");
    }
}