using System;
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
        FieldInfo fi = typeof(Actor).GetField("messageBuffer",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Queue<Message> messageBuffer = fi.GetValue(target) as Queue<Message>;

        foreach (State state in states)
        {
            EditorGUILayout.LabelField(
                $"[State ({state.Group}): {state}] Priority: {state.Priority} Active: {state.time}");
        }

        foreach (Message message in messageBuffer)
        {
            EditorGUILayout.LabelField($"[Message: {message}]");
        }
    }
}