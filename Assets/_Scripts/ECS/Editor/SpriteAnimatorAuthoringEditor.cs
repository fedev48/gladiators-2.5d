using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteAnimatorAuthoring))]
public class SpriteAnimatorAuthoringEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("animations"), true);

        EditorGUILayout.Space(4);

        var authoring = (SpriteAnimatorAuthoring)target;

        if (authoring.animations != null && authoring.animations.Count > 0)
        {
            var names = authoring.animations
                .Select((c, i) => string.IsNullOrWhiteSpace(c?.name) ? $"Clip {i}" : c.name)
                .ToArray();

            var initialProp = serializedObject.FindProperty("initialAnimation");
            initialProp.intValue = Mathf.Clamp(initialProp.intValue, 0, names.Length - 1);
            initialProp.intValue = EditorGUILayout.Popup("Initial Animation", initialProp.intValue, names);

            var currentProp = serializedObject.FindProperty("currentAnimation");
            currentProp.intValue = Mathf.Clamp(currentProp.intValue, 0, names.Length - 1);
            currentProp.intValue = EditorGUILayout.Popup("Current Animation", currentProp.intValue, names);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Popup("Initial Animation",  0, new[] { "— no animations —" });
            EditorGUILayout.Popup("Current Animation", 0, new[] { "— no animations —" });
            EditorGUI.EndDisabledGroup();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
