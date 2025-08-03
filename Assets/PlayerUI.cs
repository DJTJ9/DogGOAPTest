using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text gameTimer;
    
    [SerializeField]
    private TMP_Text interactionText;

    [SerializeField]
    private float interactionRange = 2.5f;
    
    [SerializeField]
    private GameManager gameManager;

    private void Update() {
        UpdateInteractionText();
        TimeSpan time = TimeSpan.FromSeconds(gameManager.gameTimer.CurrentTime);
        gameTimer.text = $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
    }

    private void UpdateInteractionText() {
        Ray ray = new Ray(transform.position, transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange)) {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null) {
                interactionText.text = interactable.GetInteractionName();
                return;
            }
        }
        
        interactionText.text = "";
    }
}
