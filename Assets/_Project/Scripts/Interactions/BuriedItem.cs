using System;
using UnityEngine;
using DG.Tweening;

namespace _Project.Scripts.Interactions
{
    public class BuriedItem : MonoBehaviour, IDiggable
    {
        private Rigidbody rb;
        private BoxCollider boxCollider;

        private void Awake() {
            rb = GetComponent<Rigidbody>();
            boxCollider = GetComponent<BoxCollider>();
        }

        public void PopUp() {
            if (transform.position.y < 0f) {
                transform.DOMoveY(transform.position.y + 0.2f, 2f).WaitForCompletion();
                rb.useGravity = true;
                boxCollider.enabled = true;
            }
        }
    }
}