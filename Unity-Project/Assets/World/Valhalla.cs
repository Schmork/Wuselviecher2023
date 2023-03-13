using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    static readonly string VCHANCE = "Chance ";

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

    [SerializeField]
    Slider
        distanceTravelled,
        massEaten,
        timeSurvived,
        speedEaten,
        averageSpeed,
        massPerAction,
        straightMass,
        peaceTime;

    static Valhalla Instance;

    float GetChance(Metric metric)
    {
        var value = metric switch
        {
            Metric.DistanceTravelled => distanceTravelled.value,
            Metric.MassEaten => massEaten.value,
            Metric.TimeSurvived => timeSurvived.value,
            Metric.MassEatenAtSpeed => speedEaten.value,
            Metric.AverageSpeed => averageSpeed.value,
            Metric.MassPerAction => massPerAction.value,
            Metric.StraightMass => straightMass.value,
            Metric.PeaceTime => peaceTime.value,
            _ => throw new NotImplementedException()
        };
        return value;
    }

    void OnEnable()
    {
        foreach (Metric metric in Enum.GetValues(typeof(Metric)))
        {
            string key = VCHANCE + metric.ToString();
            _ = metric switch
            {
                Metric.DistanceTravelled => distanceTravelled.value = PlayerPrefs.GetFloat(key),
                Metric.MassEaten => massEaten.value = PlayerPrefs.GetFloat(key),
                Metric.MassPerAction => massPerAction.value = PlayerPrefs.GetFloat(key),
                Metric.AverageSpeed => averageSpeed.value = PlayerPrefs.GetFloat(key),
                Metric.MassEatenAtSpeed => speedEaten.value = PlayerPrefs.GetFloat(key),
                Metric.TimeSurvived => timeSurvived.value = PlayerPrefs.GetFloat(key),
                Metric.StraightMass => straightMass.value = PlayerPrefs.GetFloat(key),
                Metric.PeaceTime => peaceTime.value = PlayerPrefs.GetFloat(key),
                _ => throw new NotImplementedException()
            };
        }

        distanceTravelled.onValueChanged.AddListener(value => SaveChance(Metric.DistanceTravelled, value));
        massEaten.onValueChanged.AddListener(value => SaveChance(Metric.MassEaten, value));
        massPerAction.onValueChanged.AddListener(value => SaveChance(Metric.MassPerAction, value));
        averageSpeed.onValueChanged.AddListener(value => SaveChance(Metric.AverageSpeed, value));
        speedEaten.onValueChanged.AddListener(value => SaveChance(Metric.MassEatenAtSpeed, value));
        timeSurvived.onValueChanged.AddListener(value => SaveChance(Metric.TimeSurvived, value));
        straightMass.onValueChanged.AddListener(value => SaveChance(Metric.StraightMass, value));
        peaceTime.onValueChanged.AddListener(value => SaveChance(Metric.PeaceTime, value));

        Instance = this;
        InitDictionaries();
        LoadHeroesFromFiles();
    }

    void SaveChance(Metric metric, float value)
    {
        PlayerPrefs.SetFloat(VCHANCE + metric.ToString(), value);
    }

    void InitDictionaries()
    {
        Heroes = new Dictionary<Metric, HeroData>();
        foreach (Metric metric in Enum.GetValues(typeof(Metric)))
        {
            Heroes.Add(metric, new HeroData() { Networks = new NeuralNetwork[4] });
        }
    }

    public void LoadHeroesFromFiles()
    {
        foreach (Metric metric in Enum.GetValues(typeof(Metric)))
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
            foreach (Metric metric in Enum.GetValues(typeof(Metric)))
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

        foreach (Metric metric in Enum.GetValues(typeof(Metric)))
        {
            Heroes[metric].Score *= (1 - Dashboard.Decay);
            OnHighscoreChanged?.Invoke(metric, Heroes[metric].Score);
        }
    }

    void OnApplicationQuit()
    {
        foreach (Metric metric in Enum.GetValues(typeof(Metric)))
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
        Dashboard.UpdateCellMaxGen(OldestGen);
        InitDictionaries();

        foreach (Metric metric in Enum.GetValues(typeof(Metric)))
        {
            OnHighscoreChanged?.Invoke(metric, 0);
            FileHandler.SaveHeroToFile(metric, 0, null);
        }
    }

    public static NeuralNetwork[] GetRandomHero()
    {
        var mutateMe = Utility.Random.NextInt(4);
        var sum = 0f;
        foreach (Metric metric in Enum.GetValues(typeof(Metric)))
        {
            if (Heroes[metric].Networks[mutateMe] == null) continue;
            sum += Instance.GetChance(metric);
        }
        var rand = Utility.Random.NextDouble() * sum;

        sum = 0;
        foreach (Metric metric in Enum.GetValues(typeof(Metric)))
        {
            var networks = Heroes[metric].Networks;
            if (networks[mutateMe] == null) continue;
            sum += Instance.GetChance(metric);
            if (rand < sum)
            {
                networks[mutateMe].Mutate(WorldConfig.GaussStd);

                if (networks[mutateMe].generation > OldestGen)
                {
                    OldestGen = networks[mutateMe].generation;
                    Dashboard.UpdateCellMaxGen(OldestGen);
                }

                return networks;
            }
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