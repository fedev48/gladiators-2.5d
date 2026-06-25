using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteAnimatorAuthoring))]
public class SpriteAnimatorAuthoringEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var auth = (SpriteAnimatorAuthoring)target;
        serializedObject.Update();

        string[] names = auth.animations != null
            ? auth.animations.Select((c, i) => $"[{i}] {c?.name ?? ""}").ToArray()
            : new string[0];

        var animsProp = serializedObject.FindProperty("animations");
        animsProp.isExpanded = EditorGUILayout.Foldout(animsProp.isExpanded, "Animations", true);
        if (animsProp.isExpanded)
        {
            EditorGUI.indentLevel++;
            int newSize = EditorGUILayout.DelayedIntField("Size", animsProp.arraySize);
            if (newSize != animsProp.arraySize) animsProp.arraySize = newSize;

            for (int i = 0; i < animsProp.arraySize; i++)
                DrawClip(animsProp.GetArrayElementAtIndex(i), i, names);

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(4);

        if (names.Length > 0)
        {
            var p = serializedObject.FindProperty("initialAnimation");
            p.intValue = Mathf.Clamp(p.intValue, 0, names.Length - 1);
            p.intValue = EditorGUILayout.Popup("Initial Animation", p.intValue, names);
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("flipPivotOffset"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraYAngle"));

        serializedObject.ApplyModifiedProperties();
    }

    static void DrawClip(SerializedProperty clip, int index, string[] allNames)
    {
        var nameProp       = clip.FindPropertyRelative("name");
        var isOverrideProp = clip.FindPropertyRelative("isOverride");
        var overrideToProp = clip.FindPropertyRelative("overrideTo");

        string label = $"[{index}] {(string.IsNullOrWhiteSpace(nameProp.stringValue) ? "Clip" : nameProp.stringValue)}";
        clip.isExpanded = EditorGUILayout.Foldout(clip.isExpanded, label, true);
        if (!clip.isExpanded) return;

        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(nameProp);

        var flipProp = clip.FindPropertyRelative("flip");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(isOverrideProp, new GUIContent("Override"));
        if (isOverrideProp.boolValue && allNames.Length > 0)
        {
            overrideToProp.intValue = EditorGUILayout.Popup(
                Mathf.Clamp(overrideToProp.intValue, 0, allNames.Length - 1), allNames);
        }
        EditorGUILayout.EndHorizontal();

        if (isOverrideProp.boolValue)
            EditorGUILayout.PropertyField(flipProp, new GUIContent("Flip X"));

        if (!isOverrideProp.boolValue)
        {
            EditorGUILayout.PropertyField(clip.FindPropertyRelative("frames"), true);
            EditorGUILayout.PropertyField(clip.FindPropertyRelative("fps"));
        }

        EditorGUI.indentLevel--;
    }
}
