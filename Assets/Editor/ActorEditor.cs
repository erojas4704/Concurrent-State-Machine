using UnityEngine;
using UnityEditor;
using CSM;
using System.Reflection;
using System.Collections.Generic;

[CustomEditor(typeof(Actor))]
public class ActorEditor : Editor
{

    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();
        StateSet states = ((Actor)target).GetStates();
        FieldInfo fi = target.GetType().GetField("actionBuffer", BindingFlags.NonPublic | BindingFlags.Instance);
        Queue<Action> actionBuffer = fi.GetValue(target) as Queue<Action>;

        foreach (State state in states)
        {
            EditorGUILayout.LabelField($"[State ({state.Group}): {state}] Priority: {state.Priority}");
        }

        foreach (Action action in actionBuffer)
        {
            EditorGUILayout.LabelField($"[Action: {action}]");
        }
    }
}