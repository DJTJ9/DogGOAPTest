using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using ScriptableValues;

public class Rat : MonoBehaviour, IDiggable
{
    [SerializeField]
    private GoapAgent goapAgent;
    
    [SerializeField]
    private ScriptableBoolValue ratIsRunning;
    
    private Animator animations;

    private void Awake() {
        animations = GetComponent<Animator>();
    }

    public void PopUp() {
     StartCoroutine(Flee());
    }

    void OnTriggerExit(Collider other) {
        if (other.transform.GetComponent<Fence>()) {
            ratIsRunning.Value = false;
        }
    }

    private IEnumerator Flee() {
        if (transform.position.y < 0f) {
            goapAgent.SetActiveRat(gameObject);
            yield return transform.DOMoveY(transform.position.y + 0.2f, 0.5f).WaitForCompletion();
        }
        
        yield return new WaitForSeconds(1f);
        animations.SetBool("isRunning", true);
        ratIsRunning.Value = true;
        
        Vector3 targetPosition = transform.position + transform.forward * 50f;
        yield return transform.DOMove(targetPosition, 20f).WaitForCompletion();
        Destroy(this);
    }
}
