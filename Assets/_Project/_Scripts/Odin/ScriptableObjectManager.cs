using Sirenix.OdinInspector.Editor;
using System;
using System.Linq;
using UnityEditor;

public class ScriptableObjectManager : OdinMenuEditorWindow
{
    private static Type[] typesToDisplay = TypeCache.GetTypesWithAttribute<ManageableDataAttribute>()
        .OrderBy(m => m.Name)
        .ToArray();

    private Type selectedType;

    [MenuItem("Tools/Scriptable Objects Manager")]
    private static void OpenEditor() => GetWindow<ScriptableObjectManager>();

    protected override void OnImGUI()
    {
        //draw menu tree for SOs and other assets
        if (GUIUtils.SelectButtonList(ref selectedType, typesToDisplay))
            this.ForceMenuTreeRebuild();

        base.OnImGUI();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        if(selectedType != null)
            tree.AddAllAssetsAtPath(selectedType.Name, "Assets/_Project/Scriptable Objects", selectedType, true, true);
        return tree;
    }
}

public class ManageableDataAttribute : Attribute { }