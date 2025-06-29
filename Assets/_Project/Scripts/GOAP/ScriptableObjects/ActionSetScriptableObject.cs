using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Action Set", menuName = "GOAP/Action Set")]
public class ActionSetScriptableObject : ScriptableObject
{
    [SerializeField] private List<ActionScriptableObject> actions = new List<ActionScriptableObject>();

    public List<ActionScriptableObject> Actions => actions;
}
