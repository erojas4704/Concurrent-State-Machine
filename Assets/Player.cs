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

    public InputActionMap actionMap;
    // Start is called before the first frame update
    void Start()
    {
        actor = GetComponent<Actor>();
        actor.EnterState<InMotion>();
        actor.EnterState<Airborne>();

        actionMap.Enable();
        actionMap.actionTriggered += OnAction;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnAction(InputAction.CallbackContext context)
    {
        Action action = new Action(context.action);
        actor.FireAction(action);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Action action = new Action();
        action.name = context.action.name;
        action.phase = Action.TranslateToActionPhase(context.phase);
        action.SetValue(context.ReadValue<Vector2>());
        actor.FireAction(action);
    }
}
