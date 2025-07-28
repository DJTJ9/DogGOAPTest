using UnityEngine;
using UnityEngine.UI;

public class Waterbarrel : MonoBehaviour, IInteractable
{
    [SerializeField]
    private Slider waterSlider;
    public void Interact() {
       waterSlider.value += 25f;
    }
}
