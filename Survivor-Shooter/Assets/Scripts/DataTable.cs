using CsvHelper;
using NUnit.Framework;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public abstract class DataTable
{
    public static readonly string FormatPath = "DataTables/{0}";
    public abstract void Load(string filename);

    //자주사용하기에 static으로 재사용
    public static List<T> LoadCSV<T>(string csvText)
    {
        using (var reader = new StringReader(csvText))
        using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture)) //지역설정무시
        {
            var records = csvReader.GetRecords<T>();
            return records.ToList();
        }
    }
    
}
