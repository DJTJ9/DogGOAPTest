    using RayFire;
    using UnityEngine;
    using UnityEngine.UI;

    public class Obstacle : MonoBehaviour, IDamagable, IInteractable
    {
        public float Health = 100f;

        [HideInInspector]
        public float actionCostIncrease;
        
        [SerializeField]
        private Slider healthSlider;
        
        [SerializeField]
        private string interactionName;
        
        [SerializeField]
        private GameObject canvas;
        
        [SerializeField]
        private Transform targetPosition;
        
        private Camera cam;
        
        private RayfireRigid rigidComponent;

        private void Awake() {
            cam = Camera.main;
            rigidComponent = GetComponent<RayfireRigid>();
        }
        
        private void Update() {
            healthSlider.value = Health;
            if (Health <= 0) {
                interactionName = "";
                canvas.SetActive(false);
                actionCostIncrease = 100;
            }
            else if (Health >= 99.9f) interactionName = "";
            else {
                interactionName = "Repair";
                actionCostIncrease = 0;
            }
        }
        
        private void LateUpdate() {
            canvas.transform.LookAt(cam.transform.position); // transform.position + cam.transform.forward
        }

        public string GetInteractionName() {
            return interactionName;
        }

        public void Interact() {
            TakeDamage(-20f);
        }

        public void TakeDamage(float damage) {
            Health -= damage;
            if (Health <= 0) Demolish();
        }

        public void Demolish() {
            if (rigidComponent == null) rigidComponent = gameObject.AddComponent<RayfireRigid>();
            
            rigidComponent.simulationType = SimType.Dynamic;
            rigidComponent.demolitionType = DemolitionType.Runtime;
            rigidComponent.objectType = ObjectType.MeshRoot;
            rigidComponent.Initialize();
            rigidComponent.Demolish();
        }

        public Transform GetTargetPosition() {
            return targetPosition;
        }
    }
