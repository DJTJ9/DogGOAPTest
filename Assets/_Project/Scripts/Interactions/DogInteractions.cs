using UnityEngine;

namespace _Project.Scripts.Interactions
{
    public class DogInteractions : MonoBehaviour, IInteractable
    {
        private DogStatus dogStatus;
        
        private void Start() {
            dogStatus = GetComponent<DogStatus>();
        }
        
        public void Interact() {
            StartCoroutine(dogStatus.ShowStatus());
        }
    }
}