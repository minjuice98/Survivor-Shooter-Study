using UnityEngine;
using TMPro;

[ExecuteInEditMode] //runtime 외에도 edit 중에도 호출(미리보기)
[RequireComponent(typeof(TextMeshProUGUI))] 
public class LocalizationText : MonoBehaviour
{
    public string stringId;

#if UNITY_EDITOR
    public Languages editorLang;
#endif
    private TextMeshProUGUI text;

    private void Awake()
    {
        text= GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            OnChangeLanguage();
        }
        else
        {
            OnChangeLanguage(editorLang);
        }
#else
#endif
    }

    public void OnChangeLanguage()
    {
        var stringTable = DataMgr.StringTable;
        text.text = stringTable.Get(stringId);
    }
#if UNITY_EDITOR
    public void OnChangeLanguage(Languages lang) //원하는 lang 설정가능버전
    {
        var tableId = DataTableIds.StringTableIds[(int)lang];
        var stringTable = DataMgr.Get<StringTable>(tableId);
        text.text = stringTable.Get(stringId);
    }
#endif
}
