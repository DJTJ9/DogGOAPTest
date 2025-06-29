using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class CreateSleepStrategyAsset
{
    [MenuItem("Assets/Create/GOAP/Actions/Sleep Action")]
    public static void CreateSleepAction()
    {
        // Sleep Strategy Scriptable Object erstellen
        var sleepStrategy = ScriptableObject.CreateInstance<SleepStrategyScriptableObject>();
        AssetDatabase.CreateAsset(sleepStrategy, "Assets/_Project/ScriptableObjects/GOAP/Strategies/SleepStrategy.asset");

        // Action Scriptable Object erstellen
        var sleepAction = ScriptableObject.CreateInstance<ActionScriptableObject>();

        // Action mit Standardwerten konfigurieren
        SerializedObject serializedAction = new SerializedObject(sleepAction);
        serializedAction.FindProperty("actionName").stringValue = "Sleep";
        serializedAction.FindProperty("strategy").objectReferenceValue = sleepStrategy;
        serializedAction.FindProperty("cost").intValue = 1;

        // Für Effects und Preconditions müsste der Editor angepasst werden, um sie zu setzen
        serializedAction.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(sleepAction, "Assets/_Project/ScriptableObjects/GOAP/Actions/SleepAction.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Sleep Strategy und Action wurden erstellt!");
    }
}
#endif
