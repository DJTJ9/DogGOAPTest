using System;
using UnityEngine;
using DG.Tweening;

namespace _Project.Scripts.Interactions
{
    public class BuriedItem : MonoBehaviour, IDiggable
    {
        public void PopUp() {
            transform.DOMoveY(transform.position.y + 1f, 0.5f).WaitForCompletion();
            // transform.DOShakePosition(5f);
        }
    }
}