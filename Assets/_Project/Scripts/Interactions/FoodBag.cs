using UnityEngine;
using UnityEngine.UI;

public class FoodBag : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Slider foodSlider;
    [SerializeField]
    private string interactionName;

    public string GetInteractionName() {
        return interactionName;
    }
    
    public void Interact() {
        foodSlider.value += 25f;
    }
}
