using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeroData
{
    public float Score;
    public NeuralNetwork[] Networks;
    public float ChanceToBePicked;
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

    public static Dictionary<Metric, HeroData> Heroes;
    public static int OldestGen = 0;

    public delegate void OnHighscoreChangedHandler(Metric metric, float newScore);
    public static event OnHighscoreChangedHandler OnHighscoreChanged;

    void OnEnable()
    {
        InitDictionaries();
        LoadHeroesFromFiles();
    }

    void InitDictionaries()
    {
        Heroes = new Dictionary<Metric, HeroData>();
        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            Heroes.Add(metric, new HeroData() { Networks = new NeuralNetwork[4] });
        }
    }

    public void LoadHeroesFromFiles()
    {
        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            var data = FileHandler.LoadHeroFromFile(metric);
            if (data == null) continue;
            Heroes[metric].Networks = data.Value.Networks;
            Heroes[metric].Score = data.Value.Score;
        }
    }

    public static void AddHero(NeuralNetwork[] networks, Metric metric, float score)
    {
        if (score < Heroes[metric].Score) return;

        var clones = new NeuralNetwork[networks.Length];
        for (int i = 0; i < clones.Length; i++)
        {
            clones[i] = networks[i].Clone() as NeuralNetwork;
            clones[i].generation++;
        }

        Heroes[metric].Networks = clones;
        Heroes[metric].Score = score;

        OnHighscoreChanged?.Invoke(metric, score);
    }

    System.Collections.IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(WorldConfig.Instance.AutoSaveMinutes * 60);
        while (true)
        {
            foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
            {
                FileHandler.SaveHeroToFile(metric, Heroes[metric].Score, Heroes[metric].Networks);
            }
            yield return new WaitForSecondsRealtime(WorldConfig.Instance.AutoSaveMinutes * 60);
        }
    }

    float lastDecay;
    void Update()
    {
        if (Time.time - lastDecay < 1) return;
        lastDecay = Time.time;

        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            Heroes[metric].Score *= (1 - Dashboard.Decay);
            OnHighscoreChanged?.Invoke(metric, Heroes[metric].Score);
        }
    }

    void OnApplicationQuit()
    {
        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            FileHandler.SaveHeroToFile(metric, Heroes[metric].Score, Heroes[metric].Networks);
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
            FileHandler.SaveHeroToFile(metric, 0, null);
        }
    }

    public static NeuralNetwork[] GetRandomHero()
    {
        var sum = 0f;
        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            sum += Heroes[metric].ChanceToBePicked;
        }
        var rand = Utility.Random.NextDouble() * sum;

        sum = 0;
        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            sum += Heroes[metric].ChanceToBePicked;
            if (rand < sum) return Heroes[metric].Networks;
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