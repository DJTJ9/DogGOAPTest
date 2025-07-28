using UnityEngine;
using UnityEngine.UI;

public class FoodBag : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Slider foodSlider;
    
    public void Interact() {
        foodSlider.value += 25f;
    }
}
