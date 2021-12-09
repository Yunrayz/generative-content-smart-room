using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class ObjectStateHandler : MonoBehaviour
{

    protected string _currentState = "";
    public State[] states;

    public string CurrentState
    {
        get
        {
            return _currentState;
        }
        set
        {
            if (_currentState == "") { Debug.LogError("State not initialized, call ObjectStateHandler Start function!"); return; }
            if (_currentState != value)
            {
                Array.Find(states, el => el.name == _currentState).Deactivate();
                _currentState = value;
                Array.Find(states, el => el.name == value).Activate();
                Debug.Log(gameObject.name + " has state " + _currentState);
                EventManager.TriggerEvent($"OnState-{_currentState}", gameObject);
            }
        }
    }

    protected void initState()
    {
        foreach (var state in states)
        {
            state.Deactivate();
        }
        _currentState = states[0].name;
        states[0].Activate();
        EventManager.TriggerEvent($"OnState-{_currentState}", gameObject);
    }
    protected virtual void Start()
    {
        initState();
        Debug.Log("Stato inizializzato");
    }

    protected virtual void Update()
    {
        //Updates the state when the order of states is changed in editorMode
        if (!Application.isPlaying &&  _currentState != states[0].name)
        {
            initState();
        }
    }
}
