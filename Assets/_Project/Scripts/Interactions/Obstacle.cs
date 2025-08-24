    using RayFire;
    using UnityEngine;
    using UnityEngine.UI;

    public class Obstacle : MonoBehaviour, IDamagable, IInteractable
    {
        public float Health = 100f;
        
        [SerializeField]
        private float repairAmount = 20f;

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
        
        private float fullHealth = 100f;
        private void Awake() {
            cam = Camera.main;
            rigidComponent = GetComponent<RayfireRigid>();
            Health = fullHealth;
            
            healthSlider.value = Health;
        }
        
        private void Update() {
            UpdateHealthSlider();
            UpdateInteractionName();
        }
        
        private void LateUpdate() {
            canvas.transform.LookAt(cam.transform.position);
        }

        private void UpdateHealthSlider() {
            healthSlider.value = Mathf.Clamp(Health, 0f, 100f);
        }

        private void UpdateInteractionName() {
            if (Health <= 0) {
                interactionName = "";
                canvas.SetActive(false);
                actionCostIncrease = 100;
            }
            else if (Health >= 100f) interactionName = "";
            else {
                interactionName = "Repair";
                actionCostIncrease = 0;
            }
        }

        public string GetInteractionName() {
            return interactionName;
        }

        public void Interact() {
            if (healthSlider.value < fullHealth)
            {
                TakeDamage(repairAmount * -1);
            }
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
