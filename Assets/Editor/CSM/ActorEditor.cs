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
        StateStack states = ((Actor)target).GetStates();
        FieldInfo fi = target.GetType() == typeof(Actor) ?
            target.GetType().GetField("actionBuffer", BindingFlags.NonPublic | BindingFlags.Instance) :
            target.GetType().BaseType.GetField("actionBuffer", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            
        Queue<Message> actionBuffer = fi.GetValue(target) as Queue<Message>;

        foreach (State state in states)
        {
            EditorGUILayout.LabelField($"[State ({state.Group}): {state}] Priority: {state.Priority} Active: {state.time}");
        }

        foreach (Message action in actionBuffer)
        {
            EditorGUILayout.LabelField($"[Action: {action}]");
        }
    }
}