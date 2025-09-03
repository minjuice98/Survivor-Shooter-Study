using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[Serializable]
public class PlayerFrameData
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;
}

public class PlayerRecorder : MonoBehaviour
{
    public Transform player;
    public static readonly string fileName = "PlayerRecord.json";
    public static string fileFullPath => Path.Combine(Application.persistentDataPath, fileName);
    private List<PlayerFrameData> frameData = new List<PlayerFrameData>();

    private bool isRecording = false;
    private bool isPlaying = false;
    private int playIndex = 0;

    private void Update()
    {
        if (isRecording)
        {
            frameData.Add(new PlayerFrameData
            {
                time = Time.time,
                position=player.position,
                rotation = player.rotation,
            });
        }
        if (isPlaying)
        {
            GamePlay();
        }
    }
    public void StartRecord()
    {
        frameData.Clear();
        isRecording = true;
        isPlaying = false;
        playIndex = 0;
    }

    public void StopRecord()
    {
        isRecording = false;
        var json = JsonConvert.SerializeObject(frameData, Newtonsoft.Json.Formatting.Indented,
            new Vector3Converter(), new QuternianConverter());//들여쓰기

        File.WriteAllText(fileFullPath, json);
    }

    public void PlayLoad()
    {
        if (File.Exists(fileFullPath))
        {
            string json = File.ReadAllText(fileFullPath);
            frameData = JsonConvert.DeserializeObject<List<PlayerFrameData>>(
                json, new Vector3Converter(), new QuternianConverter());
            playIndex = 0;
            isPlaying = true;
            isRecording = false;
        }
    }

    public void GamePlay() 
    {
        if (playIndex < frameData.Count)
        {
            var frame = frameData[playIndex];
            player.position = frame.position;
            playIndex++;
        }
        else
        {
            isPlaying = false;
        }
    }
}
