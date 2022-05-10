using UnityEngine;
using UnityEditor;
using CSM;

[CustomEditor(typeof(Actor))]
public class ActorEditor : Editor
{

    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();
        StateSet states = ((Actor)target).GetStates();

        foreach (State state in states)
        {
            EditorGUILayout.LabelField($"[State ({state.Group}): {state}] Priority: {state.Priority}");
        }
    }
}