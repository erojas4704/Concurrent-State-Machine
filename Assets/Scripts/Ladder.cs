using System.Collections;
using System.Collections.Generic;
using CSM;
using UnityEngine;

public class Ladder : MonoBehaviour, ITrigger
{
    public Transform start;
    public Transform end;

    public string GetTriggerAction() => "Ladder";
}
