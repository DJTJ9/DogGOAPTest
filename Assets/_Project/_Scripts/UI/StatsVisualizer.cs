using UnityEngine;
using ScriptableValues;
using TMPro;

public class StatsVisualizer : MonoBehaviour
{
    [SerializeField] private GoapAgent agent;
    [SerializeField] private bool showInGame = true;

    // Optional: Verwenden Sie UI Text oder TextMeshPro für eine schönere Anzeige
    [SerializeField] private Canvas worldSpaceCanvas;
    [SerializeField] private TextMeshProUGUI statsText;

    [Header("ScriptableValues")]
    [SerializeField] private ScriptableFloatValue hunger;
    [SerializeField] private ScriptableFloatValue thirst;
    [SerializeField] private ScriptableFloatValue stamina;

    [Header("Formatierung")]
    [SerializeField] private string format = "F1";  // Eine Nachkommastelle

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (agent == null)
        {
            agent = GetComponentInParent<GoapAgent>();
        }

        // ScriptableValues vom Agenten holen, falls nicht manuell zugewiesen
        if (hunger == null || thirst == null || stamina == null)
        {
            Debug.LogWarning("Keine ScriptableValues zugewiesen. Bitte weisen Sie diese manuell zu.");
        }

        // Initialisierung für UI-Komponenten
        if (worldSpaceCanvas != null && statsText != null)
        {
            worldSpaceCanvas.worldCamera = mainCamera;
        }
    }

    private void Update()
    {
        if (!showInGame || (hunger == null || thirst == null || stamina == null))
            return;

        // Text aktualisieren, wenn UI-Komponenten vorhanden sind
        if (statsText != null)
        {
            statsText.text = $"H: {hunger.Value.ToString(format)}\n" +
                             $"T: {thirst.Value.ToString(format)}\n" + 
                             $"S: {stamina.Value.ToString(format)}";
        }

        // Stellen Sie sicher, dass das Canvas immer zum Spieler/zur Kamera gedreht ist
        if (worldSpaceCanvas != null && mainCamera != null)
        {
            worldSpaceCanvas.transform.forward = mainCamera.transform.forward;
        }
    }

    // Fallback für den Fall, dass keine UI-Komponenten verwendet werden
    private void OnGUI()
    {
        if (!showInGame || statsText != null || (hunger == null || thirst == null || stamina == null))
            return;

        // Position über dem Agenten im Screen Space berechnen
        Vector3 screenPos = mainCamera.WorldToScreenPoint(agent.transform.position + Vector3.up * 2);

        // Nicht rendern, wenn hinter der Kamera
        if (screenPos.z < 0) return;

        // GUI-Label zeichnen
        GUI.contentColor = Color.white;
        GUI.backgroundColor = new Color(0, 0, 0, 0.7f);

        string content = $"H: {hunger.Value.ToString(format)} | " +
                          $"T: {thirst.Value.ToString(format)} | " +
                          $"S: {stamina.Value.ToString(format)}";

        GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y, 100, 30), content);
    }
}
