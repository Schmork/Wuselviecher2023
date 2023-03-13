using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class HeroData
{
    public float Score;
    public NeuralNetwork[] Networks;
}

public class Valhalla : MonoBehaviour
{
    public static readonly string VHERO = "Hero ";
    public static readonly string VSCORE = "Score";
    public static readonly string VNETWORKS = "Networks";

    public enum Metric
    {
        DistanceTravelled,
        MassEaten,
        TimeSurvived,
        MassEatenAtSpeed,
        AverageSpeed,
        MassPerAction,
        StraightMass,
        PeaceTime
    }

    static Dictionary<Metric, NeuralNetwork[]> heroes;
    static Dictionary<Metric, float> scores;
    public static int OldestGen = 0;
    public static float[] metricChanceForRandomHero = new float[System.Enum.GetValues(typeof(Metric)).Length];

    public delegate void OnHighscoreChangedHandler(Metric metric, float newScore);
    public static event OnHighscoreChangedHandler OnHighscoreChanged;

    static string dataPath;

    void OnEnable()
    {
        InitDictionaries();
        dataPath = Application.persistentDataPath + "/Heroes/";
        if (!Directory.Exists(dataPath))
            Directory.CreateDirectory(dataPath);
        LoadHeroesFromFiles();
    }

    void InitDictionaries()
    {
        for (int i = 0; i < metricChanceForRandomHero.Length; i++)
        {
            metricChanceForRandomHero[i] = 1;
        }

        heroes = new Dictionary<Metric, NeuralNetwork[]>();
        scores = new Dictionary<Metric, float>();
        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            heroes[metric] = new NeuralNetwork[4];
            scores[metric] = 0;
        }
    }

    public void LoadHeroesFromFiles()
    {
        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            string filePath = dataPath + VHERO + metric.ToString() + ".json";
            if (!File.Exists(filePath)) continue;
            string dataJson = File.ReadAllText(filePath);
            var data = JsonUtility.FromJson<HeroData>(dataJson);
            if (data == null) continue;
            scores[metric] = data.Score;
            heroes[metric] = data.Networks;
        }
    }

    public static void AddHero(NeuralNetwork[] networks, Metric metric, float score)
    {
        if (score < scores[metric]) return;

        var clones = new NeuralNetwork[networks.Length];
        for (int i = 0; i < clones.Length; i++)
        {
            clones[i] = networks[i].Clone() as NeuralNetwork;
        }

        heroes[metric] = clones;
        scores[metric] = score;

        OnHighscoreChanged?.Invoke(metric, score);
    }

    System.Collections.IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(WorldConfig.Instance.AutoSaveMinutes * 60);
        while (true)
        {
            foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
            {
                HeroData data = new HeroData { Score = scores[metric], Networks = heroes[metric] };
                string dataJson = JsonUtility.ToJson(data);
                File.WriteAllText(dataPath + VHERO + metric.ToString() + ".json", dataJson);
            }
            yield return new WaitForSecondsRealtime(WorldConfig.Instance.AutoSaveMinutes * 60);
        }
    }

    float lastDecay;
    void Update()
    {
        if (Time.time - lastDecay < 1) return;
        lastDecay = Time.time;

        for (int i = 0; i < scores.Count; i++)
        {
            scores[(Metric)i] *= (1 - Dashboard.Decay);
            OnHighscoreChanged?.Invoke(scores.ElementAt(i).Key, scores[(Metric)i]);
        }
    }

    void OnApplicationQuit()
    {
        for (int i = 0; i < scores.Count; i++)
        {
            var metric = (Metric)i;
            HeroData data = new HeroData { Score = scores[metric], Networks = heroes[metric] };
            string dataJson = JsonUtility.ToJson(data);
            File.WriteAllText(dataPath + VHERO + metric.ToString() + ".json", dataJson);
        }
    }

    public void Wipe()
    {
        var cells = FindObjectsOfType<SizeController>();
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].gameObject.SetActive(false);
        }

        OldestGen = 0;
        InitDictionaries();

        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            OnHighscoreChanged?.Invoke(metric, 0);
            HeroData data = new HeroData { Score = scores[metric], Networks = null };
            string dataJson = JsonUtility.ToJson(data);
            File.WriteAllText(dataPath + VHERO + metric.ToString() + ".json", dataJson);
        }
    }

    public static NeuralNetwork[] GetRandomHero()
    {
        int i;
        var sum = 0f;
        for (i = 0; i < metricChanceForRandomHero.Length; i++)
        {
            sum += metricChanceForRandomHero[i];
        }
        var rand = Utility.Random.NextDouble() * sum;

        sum = 0;
        for (i = 0; i < metricChanceForRandomHero.Length; i++)
        {
            sum += metricChanceForRandomHero[i];
            if (rand < sum) return heroes[(Metric)i];
        }

        return new NeuralNetwork[]
        {
            NeuralNetwork.NewRandom(),
            NeuralNetwork.NewRandom(),
            NeuralNetwork.NewRandom(),
            NeuralNetwork.NewRandom()
        };
    }
}