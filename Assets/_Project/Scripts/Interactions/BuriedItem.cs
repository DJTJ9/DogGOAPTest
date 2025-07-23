using System;
using UnityEngine;
using DG.Tweening;

namespace _Project.Scripts.Interactions
{
    public class BuriedItem : MonoBehaviour, IDiggable
    {
        public void PopUp() {
            if (transform.position.y < 0f) {
                transform.DOMoveY(transform.position.y + 1f, 0.5f).WaitForCompletion();
            }
        }
    }
}