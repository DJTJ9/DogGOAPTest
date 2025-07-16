using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ShakeBall : MonoBehaviour
{
    [SerializeField]
    private float shakeStrength = 0.3f;

    [SerializeField]
    private float shakeDuration = 6f;

    [SerializeField]
    private Transform objectGrabPoint;

    [SerializeField]
    private Transform ballTransform; // Das Transform, das geschüttelt werden soll

    [SerializeField]
    private AnimationCurve shakeCurve;

    private Vector3 originalPosition;

    private void Awake() {
        // Falls ballTransform nicht in Inspector gesetzt wurde, versuchen wir es zu finden
        if (ballTransform == null) {
            // Suchen nach einem Kind-GameObject mit dem Tag "Ball" oder ähnlichem
            // oder verwende objectGrabPoint selbst als Fallback
            ballTransform = objectGrabPoint;
        }
    }

    public void Shake() {
        StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine() {
        // Warte 2 Sekunden bevor die Animation startet
        yield return new WaitForSeconds(1f);

        // Speichere die ursprüngliche Position des objectGrabPoint
        originalPosition = objectGrabPoint.position;

        // Setze die Position des zu schüttelnden Objekts
        // auf die Position des objectGrabPoint plus Offset
        // ballTransform.position = originalPosition + new Vector3(0f, -0.2f, 0.2f);

        Tween moveWithMouth = ballTransform.DOMove(originalPosition + new Vector3(0f, -0.3f, 0.2f), 1f);
        yield return moveWithMouth.WaitForCompletion();
        
        // Führe die DOShakePosition-Animation auf dem ballTransform aus
        Tween shakeTween = ballTransform.DOShakePosition(shakeDuration, shakeStrength).SetEase(shakeCurve);
        // Warte bis die Animation abgeschlossen ist
        yield return shakeTween.WaitForCompletion();
        //
        // Tween moveBack = ballTransform.DOMove(originalPosition, 0.2f);
        // yield return moveBack.WaitForCompletion();

        // // Optional: Position nach dem Schütteln zurücksetzen
        // ballTransform.position = originalPosition;
    }
}