using System;
using System.Collections;
using UnityEngine;
using ImprovedTimers;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;

public class DogStatus : MonoBehaviour, IInteractable
{
    [SerializeField]
    private GameObject goapAgent;

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private float showDuration;

    [SerializeField]
    private float animationDuration;

    [SerializeField]
    private Image bubble1, bubble2, bubble3, bubble4;

    public AnimationCurve bubble1AppearCurve, bubble2AppearCurve, bubble3AppearCurve, bubble4AppearCurve;

    [SerializeField]
    private GameObject bedIcon, moodIcon, waterIcon, foodIcon;
    
    // private IEnumerator Start() {
    //     yield return new WaitForSeconds(2);
    //     
    //     yield return bubble1.transform.DOScale(Vector3.one, animationDuration).SetEase(bubble1AppearCurve).WaitForCompletion();
    //     yield return bubble2.transform.DOScale(Vector3.one, animationDuration).SetEase(bubble2AppearCurve).WaitForCompletion();
    //     yield return bubble3.transform.DOScale(Vector3.one, animationDuration).SetEase(bubble3AppearCurve).WaitForCompletion();
    //     yield return bubble4.transform.DOScale(Vector3.one, animationDuration).SetEase(bubble4AppearCurve).WaitForCompletion();
    //     
    //     yield return new WaitForSeconds(showDuration);
    //     
    //     yield return bubble1.transform.DOScale(Vector3.zero, animationDuration).WaitForCompletion();
    //     yield return bubble2.transform.DOScale(Vector3.zero, animationDuration).WaitForCompletion();
    //     yield return bubble3.transform.DOScale(Vector3.zero, animationDuration).WaitForCompletion();
    //     yield return bubble4.transform.DOScale(Vector3.zero, animationDuration).WaitForCompletion();
    //     
    //     Debug.Log("Tween finished!");
    // }

    private void LateUpdate() {
    bubble1.transform.LookAt(player.transform);
    bubble2.transform.LookAt(player.transform);
    bubble3.transform.LookAt(player.transform);
    bubble4.transform.LookAt(player.transform);
    bedIcon.transform.LookAt(player.transform);
    moodIcon.transform.LookAt(player.transform);
    waterIcon.transform.LookAt(player.transform);
    foodIcon.transform.LookAt(player.transform);
    }

    private IEnumerator ShowStatus() {
        yield return bubble1.transform.DOScale(Vector3.one, animationDuration).SetEase(bubble1AppearCurve).WaitForCompletion();
        yield return bubble2.transform.DOScale(Vector3.one, animationDuration).SetEase(bubble2AppearCurve).WaitForCompletion();
        yield return bubble3.transform.DOScale(Vector3.one, animationDuration).SetEase(bubble3AppearCurve).WaitForCompletion();
        yield return bubble4.transform.DOScale(Vector3.one, animationDuration).SetEase(bubble4AppearCurve);
        yield return bedIcon.transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), animationDuration).SetEase(bubble4AppearCurve);
        yield return moodIcon.transform.DOScale(new Vector3(0.35f, 0.35f, 0.35f), animationDuration).SetEase(bubble4AppearCurve);
        yield return waterIcon.transform.DOScale(new Vector3(0.4f, 0.4f, 0.4f), animationDuration).SetEase(bubble4AppearCurve);
        yield return foodIcon.transform.DOScale(new Vector3(0.4f, 0.4f, 0.4f), animationDuration).SetEase(bubble4AppearCurve);
        
        yield return new WaitForSeconds(showDuration);
        
        yield return foodIcon.transform.DOScale(Vector3.zero, animationDuration);
        yield return waterIcon.transform.DOScale(Vector3.zero, animationDuration);
        yield return moodIcon.transform.DOScale(Vector3.zero, animationDuration);
        yield return bedIcon.transform.DOScale(Vector3.zero, animationDuration);
        yield return bubble4.transform.DOScale(Vector3.zero, animationDuration).WaitForCompletion();
        yield return bubble3.transform.DOScale(Vector3.zero, animationDuration).WaitForCompletion();
        yield return bubble2.transform.DOScale(Vector3.zero, animationDuration).WaitForCompletion();
        yield return bubble1.transform.DOScale(Vector3.zero, animationDuration).WaitForCompletion();
        
        Debug.Log("Tween finished!");
    }

    public void Interact() {
        StartCoroutine(ShowStatus());
    }
}