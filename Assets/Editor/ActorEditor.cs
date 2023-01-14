using UnityEngine;
using UnityEditor;
using CSM;
using System.Reflection;
using System.Collections.Generic;

[CustomEditor(typeof(Actor), true)]
public class ActorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        StateSet states = ((Actor)target).GetStates();
        FieldInfo fi = target.GetType() == typeof(Actor) ?
            target.GetType().GetField("actionBuffer", BindingFlags.NonPublic | BindingFlags.Instance) :
            target.GetType().BaseType.GetField("actionBuffer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            
        Queue<Action> actionBuffer = fi.GetValue(target) as Queue<Action>;

        foreach (State state in states)
        {
            EditorGUILayout.LabelField($"[State ({state.group}): {state}] Priority: {state.priority} Active: {state.time}");
        }

        foreach (Action action in actionBuffer)
        {
            EditorGUILayout.LabelField($"[Action: {action}]");
        }
    }
}