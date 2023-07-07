using CSM;
using System;
using System.Reflection;
using UnityEditor;

[CustomEditor(typeof(Stats), true)]
public class StatsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        RenderFinalStatsList();
    }

    private void RenderFinalStatsList()
    {
        Stats stats = target as Stats;
        Type type = stats!.GetType();
        FieldInfo[] publicFields =
            type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic); //BindingFlags.Public |

        foreach (FieldInfo field in publicFields)
        {
            if (field.Name[0] == '_')
            {
                string statName = ExtractStatName(field.Name);
                object statObject = field.GetValue(stats);

                if (statObject != null && statObject.GetType().IsGenericType &&
                    statObject.GetType().GetGenericTypeDefinition() == typeof(Stats.Stat<>))
                {
                    FieldInfo originalValueField =
                        type.GetField(statName, BindingFlags.Instance | BindingFlags.NonPublic);
                    MethodInfo getValueMethod = statObject.GetType().GetMethod("GetValue");

                    object originalValue = originalValueField!.GetValue(stats);
                    object value = getValueMethod!.Invoke(statObject, null);

                    EditorGUILayout.LabelField($"{statName}: {originalValue} -> {value}");
                }
            }
            //EditorGUILayout.Space();
        }
    }

    private string ExtractStatName(string fieldName)
    {
        //TODO careful, if the Roslyn analyzer changes this will BREAK.
        return fieldName.Substring(1, fieldName.LastIndexOf("Stat", StringComparison.Ordinal) - 1);
    }
}