using System.Collections;
using System.Collections.Generic;
using CSM.States;
using UnityEngine;
using CSM;

[RequireComponent(typeof(Actor))]
public class Player : Entity
{
    private Actor actor;
    // Start is called before the first frame update
    void Start()
    {
        actor = GetComponent<Actor>();
        actor.EnterState<InMotion>();
        actor.EnterState<Airborne>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
