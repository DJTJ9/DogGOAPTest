using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField]
    private Slider foodSlider;

    [SerializeField]
    private Slider waterSlider;

    private void Update()
    {
        UpdateInteractionText();
        UpdateGameTimerDisplay();
        ClampResourceSliders();
    }

    private void ClampResourceSliders()
    {
        foodSlider.value = Mathf.Clamp(foodSlider.value, 0f, 100f);
        waterSlider.value = Mathf.Clamp(waterSlider.value, 0f, 100f);
    }

    private void UpdateGameTimerDisplay()
    {
        TimeSpan time = TimeSpan.FromSeconds(gameManager.GameTimer.CurrentTime);
        gameTimer.text = $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
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