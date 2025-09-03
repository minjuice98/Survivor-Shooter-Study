using UnityEngine;
using TMPro;

public class StringTableText : MonoBehaviour
{
    public string id;
    public TextMeshProUGUI textMeshPro;

    private void Start()
    {
        //var stringTable = DataMgr.Get<StringTable>("String");
        //textMeshPro.text = stringTable.Get(id);
        textMeshPro.text = DataMgr.StringTable.Get(id);
    }
}
