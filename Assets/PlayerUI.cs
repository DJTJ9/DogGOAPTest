using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text interactionText;

    [SerializeField]
    private GameObject interactionInfo;

    [SerializeField]
    private float interactionRange = 2.5f;

    private void Update() {
        Ray ray = new Ray(transform.position, transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange)) {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null) {
                interactionInfo.SetActive(true);
                interactionText.text = interactable.GetInteractionName();
                return;
            }
        }
        
        interactionText.text = "";
        interactionInfo.SetActive(false);
    }
}
