using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ScriptableValues
{
    [ManageableData, InlineEditor(Expanded = true)]
    [CreateAssetMenu(fileName = "Dog Settings", menuName = "Scriptable Objects/Dog Settings")]
    public class DogParamsSO : ScriptableObject
    {
        [FoldoutGroup("Movement Settings", expanded: false)]
        public float stoppingDistance;
        
        [FoldoutGroup("Movement Settings")]
        public float playerInRangeDistance;
        
        [FoldoutGroup("Movement Settings")]
        public float wanderRadius;

        [FoldoutGroup("Pick Up Settings", expanded: false)]
        public float pickUpDistance;
        
        [FoldoutGroup("Resting Settings", expanded: false)]
        public float restingDuration;
    }
}