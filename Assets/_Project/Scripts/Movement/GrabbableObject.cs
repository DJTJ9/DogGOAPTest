using System;
using ScriptableValues;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class GrabbableObject : MonoBehaviour, IInteractable
{
    public float ThrowSpeed = 50f;
    
    [SerializeField, Optional]
    private ScriptableBoolValue ballInHand;
    
    [HideInInspector]
    public bool objectPossessed;

    private Rigidbody objectRigidbody;
    private Camera cam;
    private Transform objectGrabPoint;

    private float lerpSpeed = 100f;

    private void Awake() {
        objectRigidbody = GetComponent<Rigidbody>();
        cam = Camera.main;
    }

    public void Grab(Transform _objectGrabPoint) {
        if (!objectPossessed) {
            objectGrabPoint = _objectGrabPoint;
            objectRigidbody.useGravity = false;
            objectRigidbody.linearVelocity = Vector3.zero;
            // objectRigidbody.isKinematic = true;
            objectPossessed = true;
        }
    }

    public void Drop() {
        if (objectGrabPoint != null && objectPossessed) {
            objectGrabPoint = null;
            objectRigidbody.useGravity = true;
            // objectRigidbody.isKinematic = false;
            objectPossessed = false;
        }
    }

    public void Throw(Vector3 _direction) {
        objectGrabPoint = null;
        objectRigidbody.useGravity = true;
        // objectRigidbody.isKinematic = false;
        objectRigidbody.AddForce(_direction * ThrowSpeed, ForceMode.Impulse);
    }

    private void FixedUpdate() {
        if (objectGrabPoint != null) {
            Vector3 newPosition = Vector3.Lerp(transform.position, objectGrabPoint.position, lerpSpeed * Time.deltaTime);
            objectRigidbody.MovePosition(newPosition);
        }
    }

    public string GetInteractionName() {
        if (ballInHand == null) return "Grab";
        
        return ballInHand.Value ? "Drop" : "Grab";
    }

    public void Interact() {
        if (ballInHand.Value) Drop();
        else Grab(objectGrabPoint);
    }
}