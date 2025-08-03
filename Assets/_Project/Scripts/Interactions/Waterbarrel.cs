using UnityEngine;
using UnityEngine.UI;

public class Waterbarrel : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Slider waterSlider;
    [SerializeField]
    private string interactionName;

    public string GetInteractionName() {
        return interactionName;
    }

    public void Interact() {
        if (waterSlider.value >= 100f) return;
       waterSlider.value += 25f;
    }
}
