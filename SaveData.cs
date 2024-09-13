using System;
using System.Collections.Generic;
using DebugMenu;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class SaveData
{
    public static string Path => Application.persistentDataPath + "/DebugMenuSaveData.json";
    
    public List<string> favouritedPerks = new List<string>();
    public List<string> favouritedGoods = new List<string>();

    public void Save()
    {
        string json = JsonConvert.SerializeObject(this);
        if (string.IsNullOrEmpty(json))
        {
            Plugin.Log.LogError("Failed to serialize save data.");
            return;
        }
        
        System.IO.File.WriteAllText(Path, json);
    }
    
    public static SaveData Load()
    {
        if (!System.IO.File.Exists(Path))
        {
            Plugin.Log.LogInfo("Save data file does not exist.");
            return new SaveData();
        }
        
        string json = System.IO.File.ReadAllText(Path);
        if (string.IsNullOrEmpty(json))
        {
            Plugin.Log.LogError("Failed to read save data!");
            return new SaveData();
        }
        
        SaveData data = JsonConvert.DeserializeObject<SaveData>(json);
        if (data == null)
        {
            Plugin.Log.LogError("Failed to deserialize save data.");
            return new SaveData();
        }
        
        return data;
    }
}