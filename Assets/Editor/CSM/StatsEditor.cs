using CSM;
using System;
using System.Linq;
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
        FieldInfo[] privateFields = GetAllFields(type);

        foreach (FieldInfo field in privateFields)
        {
            if (field.Name[0] == '_')
            {
                string statName = ExtractStatName(field.Name);
                object statObject = field.GetValue(stats);

                if (statObject != null && statObject.GetType().IsGenericType &&
                    statObject.GetType().GetGenericTypeDefinition() == typeof(Stats.Stat<>))
                {
                    FieldInfo originalValueField = GetFieldIncludingBaseTypes(type, statName);
                    MethodInfo getValueMethod = statObject.GetType().GetMethod("GetValue");

                    object originalValue = originalValueField!.GetValue(stats);
                    object value = getValueMethod!.Invoke(statObject, null);

                    EditorGUILayout.LabelField($"{statName}: {originalValue} -> {value}");
                }
            }
            //EditorGUILayout.Space();
        }
    }
    
    private static FieldInfo[] GetAllFields(Type type)
    {
        if (type == null)
            return new FieldInfo[0];

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        return type.GetFields(flags)
            .Concat(GetAllFields(type.BaseType))
            .ToArray();
    }
    
    private static FieldInfo GetFieldIncludingBaseTypes(Type type, string name)
    {
        FieldInfo field = null;
        while (type != null)
        {
            field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
                break;

            type = type.BaseType;
        }

        return field;
    }

    private string ExtractStatName(string fieldName)
    {
        //TODO careful, if the Roslyn analyzer changes this will BREAK.
        return fieldName.Substring(1, fieldName.LastIndexOf("Stat", StringComparison.Ordinal) - 1);
    }
}