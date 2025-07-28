using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Rat : MonoBehaviour, IDiggable
{
    private Animator animations;

    private void Awake() {
        animations = GetComponent<Animator>();
    }

    public void PopUp() {
     StartCoroutine(Flee());
    }

    private IEnumerator Flee() {
        if (transform.position.y < 0f) {
            yield return transform.DOMoveY(transform.position.y + 1f, 0.5f).WaitForCompletion();
        }
        
        yield return new WaitForSeconds(2f);
        animations.SetBool("isRunning", true);
        
        Vector3 targetPosition = transform.position + transform.forward * 50f;
        yield return transform.DOMove(targetPosition, 20f).WaitForCompletion();
    }
}
