using System;
using UnityEditor;
using CSM;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CustomEditor(typeof(Actor), true)]
public class ActorEditor : Editor
{
    private static List<Type> stateTypes;
    private static List<String> stateTypeNames;
    
    private int selectedIndex;
    private SerializedProperty defaultStateProperty;

    [InitializeOnLoadMethod]
    private static void InitializeStateTypes()
    {
        stateTypes = GetAllStateTypes();
        stateTypes = GetAllStateTypes();
        
        stateTypeNames = stateTypes
            .Select(type => type.FullName)
            .ToList();
    }
    
    private void OnEnable()
    {
        defaultStateProperty = serializedObject.FindProperty("defaultState");
        
        string currentStateName = defaultStateProperty.stringValue;
        selectedIndex = stateTypeNames.IndexOf(currentStateName);

        if (selectedIndex < 0)
            selectedIndex = 0;
    }

    public override void OnInspectorGUI()
    {
        selectedIndex = EditorGUILayout.Popup("Default State", selectedIndex, stateTypeNames.ToArray());
        defaultStateProperty.stringValue = stateTypeNames[selectedIndex];
        serializedObject.ApplyModifiedProperties();
        DrawDefaultInspector();

        serializedObject.Update();

        StateStack states = ((Actor)target).GetStates();
        FieldInfo fi = typeof(Actor).GetField("messageBuffer",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        FieldInfo fiHeldMessages = typeof(Actor).GetField("heldMessages",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

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

    private static List<Type> GetAllStateTypes()
    {
        return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where type.IsSubclassOf(typeof(State)) && !type.IsAbstract && type.IsPublic
            select type).ToList();
    }
}