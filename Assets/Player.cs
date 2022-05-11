using System.Collections;
using System.Collections.Generic;
using CSM.States;
using UnityEngine;
using UnityEngine.InputSystem;
using CSM;

[RequireComponent(typeof(Actor))]
public class Player : Entity
{
    private Actor actor;
    private Vector2 axis;
    public InputActionMap actionMap;

    void Start()
    {
        actor = GetComponent<Actor>();
        actor.EnterState<InMotion>();
        actor.EnterState<Airborne>();
        actionMap.Enable();
        actionMap.actionTriggered += OnAction;
    }

    void Update()
    {

    }

    public void OnAction(InputAction.CallbackContext context)
    {
        Action action = new Action(context.action);
        actor.FireAction(action);
        action.axis = axis;

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 axis = context.ReadValue<Vector2>();
        this.axis = axis;

        Action action = new Action();
        action.name = context.action.name;
        action.phase = Action.TranslateToActionPhase(context.phase);
        action.SetValue(context.ReadValue<Vector2>());
        action.axis = axis;
        actor.FireAction(action, false);
    }

    public void OnEnable()
    {
        actionMap.Enable();
        actionMap.actionTriggered += OnAction;
    }
}
