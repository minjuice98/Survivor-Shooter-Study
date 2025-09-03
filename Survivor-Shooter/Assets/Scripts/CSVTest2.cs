using UnityEngine;
using TMPro;

public class CSVTest2 :MonoBehaviour
{
    public TextMeshProUGUI text;

    private void Start()
    {
        var stringTable = new StringTable();
        stringTable.Load("StringTableKr");
        text.text = stringTable.Get("HELLO");

        Debug.Log(stringTable.Get("BYE"));
        Debug.Log(stringTable.Get("..."));
    }
}
