﻿    using RayFire;
    using UnityEngine;
    using UnityEngine.UI;

    public class Obstacle : MonoBehaviour, IDamagable
    {
        public float Health = 100f;

        [HideInInspector]
        public float actionCostIncrease;
        
        [SerializeField]
        private Slider healthSlider;
        
        [SerializeField]
        private GameObject canvas;
        
        private Camera cam;
        
        private RayfireRigid rigidComponent;

        private void Awake() {
            cam = Camera.main;
            rigidComponent = GetComponent<RayfireRigid>();
        }
        
        private void Update() {
            healthSlider.value = Health;
            if (Health <= 0) {
                canvas.SetActive(false);
                actionCostIncrease = 100;
            }
            else actionCostIncrease = 0;
        }
        
        private void LateUpdate() {
            canvas.transform.LookAt(cam.transform.position); // transform.position + cam.transform.forward
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
    }
