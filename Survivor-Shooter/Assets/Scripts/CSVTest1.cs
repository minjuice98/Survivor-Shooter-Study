using UnityEngine;
using CsvHelper;
using System.IO;
using System.Globalization;

public class CsvData
{
    public string Id { get; set; }
    public string String { get; set; }
}

public class CSVTest1 : MonoBehaviour
{
    public TextAsset csv;
    private void Start()
    {
        TextAsset csv = Resources.Load<TextAsset>("DataTables/StringTableKr");

        using (var reader = new StringReader(csv.text))
        using (var csvReader=new CsvReader(reader, CultureInfo.InvariantCulture)) //지역설정무시
        {
            var records = csvReader.GetRecords<CsvData>();
            foreach (var record in records)
            {
                Debug.Log($"{record.Id}:{record.String}");
            }
        }
        Debug.Log(csv.text);
    }
}