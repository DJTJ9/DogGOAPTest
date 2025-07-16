using System;
using UnityEngine;
using DG.Tweening;

namespace _Project.Scripts.Interactions
{
    public class BoneItem : MonoBehaviour, IDiggable
    {
        public void PopUp() {
            transform.DOShakePosition(5f);
        }
    }
}