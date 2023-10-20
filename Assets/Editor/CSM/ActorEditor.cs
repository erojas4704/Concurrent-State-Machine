using System;
using System.Collections;
using UnityEditor;
using CSM;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

//TODO z-57 - - Clean this up
[CustomEditor(typeof(Actor), true)]
public class ActorEditor : Editor
{
    private static List<Type> stateTypes;
    private static List<string> stateTypeNames;
    private static string[] namespaces;

    private int selectedIndex;
    private int selectedNamespaceIndex;
    private SerializedProperty defaultStateProperty;

    [InitializeOnLoadMethod]
    private static void InitializeStateTypes()
    {
        stateTypes = GetAllStateTypes();
        namespaces = stateTypes.Select(type => type.Namespace)
            .Distinct()
            .OrderBy(ns => ns)
            .ToArray();

        stateTypeNames = stateTypes
            .Select(type => type.FullName)
            .ToList();
    }

    private void OnEnable()
    {
        defaultStateProperty = serializedObject.FindProperty("defaultState");
        UpdateSelectedIndices();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        DrawDefaultStateSelector();
        DrawStates();
        DrawMessages();
        DrawGhostStates();
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGhostStates()
    {
        Type ghostStateType = typeof(Actor).GetNestedType("GhostState", BindingFlags.NonPublic);
        FieldInfo ghostStateField = typeof(Actor).GetField("ghostStates",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        IDictionary dictionary = (IDictionary)ghostStateField!.GetValue(target);
        ICollection ghostStates = dictionary.Values;

        if (ghostStates.Count < 1)
            return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ghost States", EditorStyles.boldLabel);
        foreach (object o in ghostStates)
        {
            FieldInfo stateField = ghostStateType.GetField("state", BindingFlags.Public | BindingFlags.Instance);
            FieldInfo messagesField =
                ghostStateType.GetField("messagesToListenFor", BindingFlags.Public | BindingFlags.Instance);

            State state = stateField!.GetValue(o) as State;
            HashSet<string> messages = messagesField!.GetValue(o) as HashSet<string>;
            DrawGhostState(state, messages);
        }
    }

    private void DrawGhostState(State state, HashSet<string> messages)
    {
        EditorGUILayout.LabelField(
            $@"<{state!.GetType().Name}> ::: Time: {state.expiresAt - Time.time} Listening for: {messages.Aggregate("", (current, next) => current + next + " ")}");
    }

    private void DrawDefaultStateSelector()
    {
        selectedNamespaceIndex = EditorGUILayout.Popup("Behavior Set", selectedNamespaceIndex, namespaces);
        Type[] namespaceFilteredStateTypes = GetStateTypesInNamespace(selectedNamespaceIndex);

        string currentStateName = defaultStateProperty.stringValue;
        Type selectedStateType = stateTypes.FirstOrDefault(type => type.FullName == currentStateName);

        selectedIndex = Array.IndexOf(namespaceFilteredStateTypes, selectedStateType);
        selectedIndex = EditorGUILayout.Popup("Default State", selectedIndex,
            namespaceFilteredStateTypes.Select(type => type.Name).ToArray());
        if (selectedIndex >= 0)
            defaultStateProperty.stringValue = stateTypeNames[selectedIndex];
    }

    private void DrawStates()
    {
        StateStack states = ((Actor)target).GetStates();
        EditorGUILayout.LabelField("State Stack", EditorStyles.boldLabel);

        foreach (State state in states)
        {
            if (IsStateHidden(state))
                continue;

            EditorGUILayout.LabelField(
                $"[State ({state.Group}): {state}] Priority: {state.Priority} Active: {state.Timer}");
        }
    }

    private void DrawMessages()
    {
        FieldInfo messageBrokerField = typeof(Actor).GetField("messageBroker",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        MessageBroker messageBroker = messageBrokerField!.GetValue(target) as MessageBroker;
        
        FieldInfo messageBufferField = typeof(MessageBroker).GetField("messageBuffer",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        FieldInfo heldMessagesField = typeof(MessageBroker).GetField("heldMessages",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);


        List<Message> messageBuffer = messageBufferField!.GetValue(messageBroker) as List<Message>;
        Dictionary<string, Message> heldMessages =
            heldMessagesField!.GetValue(messageBroker) as Dictionary<string, Message>;

        Debug.Assert(messageBuffer != null, nameof(messageBuffer) + " != null");
        if (messageBuffer.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Buffered Messages", EditorStyles.boldLabel);

            foreach (Message message in messageBuffer)
            {
                EditorGUILayout.LabelField($"[Message: {message}]");
            }
        }

        if (heldMessages != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Held Inputs", EditorStyles.boldLabel);
            foreach (KeyValuePair<string, Message> keyValuePair in heldMessages)
            {
                EditorGUILayout.LabelField($"[{keyValuePair.Key}] {keyValuePair.Value}");
            }
        }
    }

    private static bool IsStateHidden(State state)
    {
        StateDescriptor stateDescriptor =
            Attribute.GetCustomAttribute(state.GetType(), typeof(StateDescriptor)) as StateDescriptor;
        return stateDescriptor is { hidden: true };
    }

    private Type[] GetStateTypesInNamespace(int namespaceIndex)
    {
        return stateTypes
            .Where(type => type.Namespace == namespaces[namespaceIndex])
            .ToArray();
    }

    private void UpdateSelectedIndices()
    {
        string currentStateName = defaultStateProperty.stringValue;

        Type selectedStateType = stateTypes.FirstOrDefault(type => type.FullName == currentStateName);
        if (selectedStateType != null)
            selectedNamespaceIndex = Array.IndexOf(namespaces, selectedStateType.Namespace);
        else
            selectedNamespaceIndex = -1;


        if (selectedIndex < 0)
            selectedIndex = 0;
    }

    private static List<Type> GetAllStateTypes()
    {
        return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where type.IsSubclassOf(typeof(State)) && !type.IsAbstract && type.IsPublic
            select type).ToList();
    }
}