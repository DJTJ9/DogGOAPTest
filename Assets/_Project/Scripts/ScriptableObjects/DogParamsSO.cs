using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableValues
{
    [CreateAssetMenu(fileName = "Dog Params", menuName = "Scriptable Objects/Dog Parameters")]
    public class DogParamsSO : ScriptableObject
    {
        [FoldoutGroup("Settings", expanded: false)]
        public float wanderRadius;
        
        [FoldoutGroup("Settings")]
        public float pickUpDistance;
        
        [FoldoutGroup("Settings")]
        public float restingDuration;
    }
}