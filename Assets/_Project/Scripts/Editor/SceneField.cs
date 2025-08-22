using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneField
{
    [SerializeField]
    private GameObject sceneAsset;

    [SerializeField]
    private string sceneName = "";

    public string SceneName { get => sceneName; }

    // Allows to work with existing Unity methods
    public static implicit operator string(SceneField sceneField)
    {
        return sceneField.SceneName;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);

            SerializedProperty sceneAssetProperty = property.FindPropertyRelative("sceneAsset");
            SerializedProperty sceneNameProperty = property.FindPropertyRelative("sceneName");

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            if (sceneAssetProperty != null)
            {
                sceneAssetProperty.objectReferenceValue = EditorGUI.ObjectField(position, sceneAssetProperty.objectReferenceValue, typeof(SceneAsset), false);

                if (sceneAssetProperty.objectReferenceValue != null)
                {
                    sceneNameProperty.stringValue = (sceneAssetProperty.objectReferenceValue as SceneAsset).name;
                }
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}