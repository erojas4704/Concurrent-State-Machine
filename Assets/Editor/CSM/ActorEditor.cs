using System;
using UnityEditor;
using CSM;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

[CustomEditor(typeof(Actor), true)]
public class ActorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        StateStack states = ((Actor)target).GetStates();
        FieldInfo fi = typeof(Actor).GetField("messageBuffer",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        FieldInfo fiGhostStates = typeof(Actor).GetField("ghostStates",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        FieldInfo fiHeldMessages = typeof(Actor).GetField("heldMessages",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        List<State> ghostStates = fiGhostStates!.GetValue(target) as List<State>;

        Queue<Message> messageBuffer = fi.GetValue(target) as Queue<Message>;
        Dictionary<string, Message> heldMessages = fiHeldMessages.GetValue(target) as Dictionary<string, Message>;

        foreach (State state in states)
        {
            EditorGUILayout.LabelField(
                $"[State ({state.Group}): {state}] Priority: {state.Priority} Active: {state.Timer}");
        }

        foreach (Message message in messageBuffer)
        {
            EditorGUILayout.LabelField($"[Message: {message}]");
        }

        EditorGUILayout.Space();
        if (heldMessages != null)
            foreach (KeyValuePair<string, Message> keyValuePair in heldMessages)
            {
                EditorGUILayout.LabelField($"[{keyValuePair.Key}] {keyValuePair.Value}");
            }
    }
}