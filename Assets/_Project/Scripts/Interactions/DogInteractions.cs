using System;
using UnityEngine;

public class DogInteractions : MonoBehaviour, IInteractable, ICommandable
{
    [SerializeField]
    private DogSO dog;
    
    private DogStatus dogStatus;
    
    [SerializeField]
    private string interactionName;

    private bool treatWasGiven = false;

    public string GetInteractionName() {
        return interactionName;
    }

    private void Start() {
        dogStatus = GetComponent<DogStatus>();
    }

    private void Update() {
        if (!dog.SeekingAttention) treatWasGiven = false;
    }

    public void Interact() {
        if (dog.SeekingAttention && !treatWasGiven) {
            dog.Fun += 25f;
            dog.Aggression -= 25f;
            dog.Satiety += 20f;
            treatWasGiven = true;
        }
        else if (!dog.SeekingAttention) {
            StartCoroutine(dogStatus.ShowStatus());
        }
    }

    public void ExecuteCommand() {
        dog.DogCalled = !dog.DogCalled;
    }
}