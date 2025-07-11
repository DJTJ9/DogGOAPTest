// using UnityEngine;
// using UnityEditor;
// using ScriptableValues;
//
// #if UNITY_EDITOR
// [CustomEditor(typeof(ScriptableFloatValue))]
// public class ScriptableFloatValueEditor : Editor
// {
//     private bool showDebugInfo = true;
//
//     public override void OnInspectorGUI()
//     {
//         // Standard-Inspector zeichnen
//         DrawDefaultInspector();
//
//         // Referenz auf das ScriptableFloatValue
//         ScriptableFloatValue floatValue = (ScriptableFloatValue)target;
//
//         // Trennlinie
//         EditorGUILayout.Space();
//         EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
//
//         // Debug-Bereich mit Faltfunktion
//         showDebugInfo = EditorGUILayout.Foldout(showDebugInfo, "Laufzeit-Informationen", true);
//
//         if (showDebugInfo)
//         {
//             // Nur im Play-Mode kann der Wert angezeigt und geändert werden
//             EditorGUI.BeginDisabledGroup(!Application.isPlaying);
//
//             // Aktueller Wert mit Schieberegler
//             EditorGUI.BeginChangeCheck();
//             float newValue = EditorGUILayout.Slider("Aktueller Wert", floatValue.Value, 
//                 floatValue.MinValue, floatValue.MaxValue);
//
//             if (EditorGUI.EndChangeCheck())
//             {
//                 // Wert ändern, wenn Schieberegler bewegt wurde
//                 floatValue.Value = newValue;
//             }
//
//             // Schaltflächen für häufige Operationen
//             EditorGUILayout.BeginHorizontal();
//
//             if (GUILayout.Button("Auf 0 setzen"))
//             {
//                 floatValue.Value = 0f;
//             }
//
//             if (GUILayout.Button("Auf Max setzen"))
//             {
//                 floatValue.Value = floatValue.MaxValue;
//             }
//
//             if (GUILayout.Button("Reset"))
//             {
//                 floatValue.ResetToDefault();
//             }
//
//             EditorGUILayout.EndHorizontal();
//
//             EditorGUI.EndDisabledGroup();
//
//             if (!Application.isPlaying)
//             {
//                 EditorGUILayout.HelpBox("Werte können nur im Play-Mode geändert werden.", 
//                     MessageType.Info);
//             }
//         }
//
//         // Sicherstellen, dass Änderungen gespeichert werden
//         serializedObject.ApplyModifiedProperties();
//     }
// }
// #endif
