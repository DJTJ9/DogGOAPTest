using UnityEngine;

public class DogInteractions : MonoBehaviour, IInteractable, ICommandable
{
    [SerializeField]
    private DogSO dog;
    
    private DogStatus dogStatus;

    private void Start() {
        dogStatus = GetComponent<DogStatus>();
    }

    public void Interact() {
        if (dog.SeekingAttention) {
            dog.Fun += 25f;
            dog.Aggression -= 25f;
            dog.Satiety += 20f;
        }
        else StartCoroutine(dogStatus.ShowStatus());
    }

    public void ExecuteCommand() {
        dog.DogCalled = !dog.DogCalled;
    }
}