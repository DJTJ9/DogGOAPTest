using System;
using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    public GameEvent Event;
    
    public UnityEvent Response;

    private void OnEnable() {
        Event.RegisterListener(this);
    }

    void OnDisable() {
        Event.UnregisterListener(this);
    }

    public void OnEventRaised() {
        Response.Invoke();       
    }
}
