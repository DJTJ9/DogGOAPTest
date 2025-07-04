using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableValues
{
    [ManageableData]
    [CreateAssetMenu(fileName = "Dog Params", menuName = "Scriptable Objects/Dog Parameters")]
    public class DogParamsSO : ScriptableObject
    {
        [FoldoutGroup("Settings", expanded: true)]
        public float wanderRadius;
        
        [FoldoutGroup("Settings")]
        public float pickUpDistance;
        
        [FoldoutGroup("Settings")]
        public float restingDuration;
    }
}