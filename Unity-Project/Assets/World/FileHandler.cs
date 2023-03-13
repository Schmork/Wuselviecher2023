using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(FileHandler))]
public class FileHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Clear Valhalla"))
        {
            foreach (var metric in System.Enum.GetValues(typeof(Valhalla.Metric)))
            {
                string filePath = FileHandler.DataPath + Valhalla.VHERO + metric.ToString() + ".json";
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            Debug.Log("Valhalla cleared.");
        }
    }
}
#endif

public class FileHandler : MonoBehaviour
{
    internal static string DataPath
    {
        get
        {
            var path = Application.persistentDataPath + "/Heroes/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    public static (float Score, NeuralNetwork[] Networks)? LoadHeroFromFile(Valhalla.Metric metric)
    {
        string filePath = DataPath + Valhalla.VHERO + metric.ToString() + ".json";
        if (!File.Exists(filePath)) return null;
        string dataJson = File.ReadAllText(filePath);
        var data = JsonUtility.FromJson<HeroData>(dataJson);
        return data == null ? null : (data.Score, data.Networks);
    }

    public static void SaveHeroToFile(Valhalla.Metric metric, float score, NeuralNetwork[] networks)
    {
        HeroData data = new HeroData { Score = score, Networks = networks };
        string dataJson = JsonUtility.ToJson(data);
        File.WriteAllText(DataPath + Valhalla.VHERO + metric.ToString() + ".json", dataJson);
    }
}