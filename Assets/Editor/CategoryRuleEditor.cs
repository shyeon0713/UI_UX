using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CategoryRule))]
public class CategoryRuleEditor : Editor
{
    SerializedProperty categoryType;
    SerializedProperty keywords;

    private void OnEnable()
    {
        categoryType = serializedObject.FindProperty("categoryType");
        keywords = serializedObject.FindProperty("keywords");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(categoryType);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Keywords", EditorStyles.boldLabel);

        for (int i = 0; i < keywords.arraySize; i++)
        {
            SerializedProperty element = keywords.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();
            element.stringValue = EditorGUILayout.TextField(element.stringValue);

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                keywords.DeleteArrayElementAtIndex(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Keyword"))
        {
            keywords.InsertArrayElementAtIndex(keywords.arraySize);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
