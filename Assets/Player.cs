using CSM.States;
using UnityEngine;
using UnityEngine.InputSystem;
using CSM;
using CSM.Entities.States;

[RequireComponent(typeof(Actor))]
public class Player : MonoBehaviour
{
    // [HideInInspector]
    public Vector2 axis;
    private Actor actor;
    public InputActionMap actionMap;

    void Start()
    {
        actor = GetComponent<Actor>();
        actor.EnterState<Airborne>();
        actor.EnterState<MeleeArmed>();
        actor.EnterState<InMotion>();
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
        axis = context.ReadValue<Vector2>();

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
