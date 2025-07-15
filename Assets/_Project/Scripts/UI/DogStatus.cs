using System;
using System.Collections;
using UnityEngine;
using ImprovedTimers;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;

public class DogStatus : MonoBehaviour
{
    [SerializeField]
    private GameObject goapAgent;

    [SerializeField]
    private GameObject cam;

    [SerializeField]
    private GameObject canvas;

    [SerializeField]
    private float showDuration;

    [SerializeField]
    private float animationDuration;

    [SerializeField]
    private Image bubble1, bubble2, bubble3, bubble4;

    public AnimationCurve bubble1AppearCurve, bubble2AppearCurve, bubble3AppearCurve, bubble4AppearCurve;

    [SerializeField]
    private GameObject bedIcon, moodIcon, waterIcon, foodIcon, heartIcon, ballIcon;
    
    [SerializeField]
    private GameObject healthBar;

    [SerializeField]
    private DogSO dog;

    private void LateUpdate() {
        canvas.transform.LookAt(cam.transform.position); // transform.position + cam.transform.forward
        Vector3 currentRotation = canvas.transform.eulerAngles;
        canvas.transform.eulerAngles = new Vector3(currentRotation.x, currentRotation.y + 180f, currentRotation.z);
    }

    public IEnumerator ShowStatus() {
        healthBar.SetActive(false);
        yield return bubble1.transform.DOScale(Vector3.one, animationDuration).SetEase(bubble1AppearCurve).WaitForCompletion();
        yield return bubble2.transform.DOScale(Vector3.one, animationDuration).SetEase(bubble2AppearCurve).WaitForCompletion();
        yield return bubble3.transform.DOScale(Vector3.one, animationDuration).SetEase(bubble3AppearCurve).WaitForCompletion();
        yield return bubble4.transform.DOScale(Vector3.one, animationDuration).SetEase(bubble4AppearCurve);
        if (dog.Stamina < 30f) 
            yield return bedIcon.transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), animationDuration).SetEase(bubble4AppearCurve);
        if (dog.Fun < 30f) 
            yield return ballIcon.transform.DOScale(new Vector3(0.35f, 0.35f, 0.35f), animationDuration).SetEase(bubble4AppearCurve);
        if (dog.Hydration < 30f) 
            yield return waterIcon.transform.DOScale(new Vector3(0.4f, 0.4f, 0.4f), animationDuration).SetEase(bubble4AppearCurve);
        if (dog.Satiety < 30f) 
            yield return foodIcon.transform.DOScale(new Vector3(0.4f, 0.4f, 0.4f), animationDuration).SetEase(bubble4AppearCurve);
        if (dog.Aggression > 50)
            yield return moodIcon.transform.DOScale(new Vector3(0.25f, 0.25f, 0.25f), animationDuration).SetEase(bubble4AppearCurve);
        if (dog.Stamina >= 30f && dog.Fun >= 30 && dog.Hydration >= 30 && dog.Satiety >= 30) 
            yield return heartIcon.transform.DOScale(new Vector3(0.75f, 0.75f, 0.75f), animationDuration).SetEase(bubble4AppearCurve);

        yield return new WaitForSeconds(showDuration);

        yield return bubble1.transform.DOScale(Vector3.zero, animationDuration).WaitForCompletion();
        yield return bubble2.transform.DOScale(Vector3.zero, animationDuration).WaitForCompletion();
        yield return bubble3.transform.DOScale(Vector3.zero, animationDuration).WaitForCompletion();
        yield return bubble4.transform.DOScale(Vector3.zero, animationDuration);
        yield return heartIcon.transform.DOScale(Vector3.zero, animationDuration);       
        yield return foodIcon.transform.DOScale(Vector3.zero, animationDuration);
        yield return waterIcon.transform.DOScale(Vector3.zero, animationDuration);
        yield return moodIcon.transform.DOScale(Vector3.zero, animationDuration);
        yield return bedIcon.transform.DOScale(Vector3.zero, animationDuration);
        yield return ballIcon.transform.DOScale(Vector3.zero, animationDuration);
        
        healthBar.SetActive(true);       
        Debug.Log("Tween finished!");
    }

    // public void Interact() {
    //     StartCoroutine(ShowStatus());
    // }
}