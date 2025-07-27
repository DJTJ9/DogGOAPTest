using UnityEngine;

public class DogLookAt : MonoBehaviour
{
    [SerializeField]
    private Transform parent;
    [SerializeField]
    private Transform lookAtTarget;
    [SerializeField]
    private float lookAtSpeed = 1f;

    private bool useLookAt;
    private Transform lookAtTransform;
    private Vector3 lookAtDirection;

    private void Update() {
        if (!useLookAt) lookAtDirection = parent.position + parent.forward * 2f;
        
        else lookAtDirection = lookAtTransform.position;
        
        lookAtTarget.transform.position = Vector3.Lerp(lookAtTarget.transform.position, lookAtDirection, lookAtSpeed * Time.deltaTime);
    }

    public void OnTriggerEnter(Collider other) {
        if (other.transform.GetComponent<PointOfInterestForDog>()) {
            useLookAt = true;
            lookAtTransform = other.transform;
        }
    }
    
    public void OnTriggerExit(Collider other) {
        if (other.transform.GetComponent<PointOfInterestForDog>()) useLookAt = false;
    }
}