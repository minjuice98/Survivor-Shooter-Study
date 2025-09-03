using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LocalizationText))]
public class LocalizationTextEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var text = target as LocalizationText;
        var newId = EditorGUILayout.TextField("String Id", text.stringId);
        text.stringId = newId;
        var newLang = (Languages)EditorGUILayout.EnumPopup("Language", text.editorLang);

        if (newId != text.stringId || newLang != text.editorLang)
        {
            text.stringId = newId;
            text.editorLang = newLang;
            text.OnChangeLanguage(text.editorLang);
            EditorUtility.SetDirty(text);
        }
    }
}
