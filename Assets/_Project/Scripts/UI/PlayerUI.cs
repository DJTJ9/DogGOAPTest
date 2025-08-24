using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text interactionText;

    [SerializeField]
    private float interactionRange = 2.5f;

    [SerializeField]
    private Slider foodSlider;

    [SerializeField]
    private Slider waterSlider;

    private void Update()
    {
        UpdateInteractionText();
        ClampResourceSliders();
    }

    private void ClampResourceSliders()
    {
        foodSlider.value = Mathf.Clamp(foodSlider.value, 0f, 100f);
        waterSlider.value = Mathf.Clamp(waterSlider.value, 0f, 100f);
    }

    private void UpdateInteractionText()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactionText.text = interactable.GetInteractionName();
                return;
            }
        }

        interactionText.text = "";
    }
}