using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class Valhalla : MonoBehaviour
{
    public static readonly string VHERO = "Valhalla Hero ";
    public static readonly string VSCOR = "Valhalla Score ";

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

    private static Dictionary<Metric, NeuralNetwork> heroes;
    private static Dictionary<Metric, float> scores;
    public static int OldestGen = 0;
    public static float[] metricChanceForRandomHero = new float[System.Enum.GetValues(typeof(Metric)).Length];

    public delegate void OnHighscoreChangedHandler(Metric metric, float newScore);
    public static event OnHighscoreChangedHandler OnHighscoreChanged;

    public void OnEnable()
    {
        InitDictionaries();
        LoadHeroesFromPrefs();
    }

    void InitDictionaries()
    {
        for (int i = 0; i < metricChanceForRandomHero.Length; i++)
        {
            metricChanceForRandomHero[i] = 1;
        }

        heroes = new Dictionary<Metric, NeuralNetwork>();
        scores = new Dictionary<Metric, float>();
        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            heroes[metric] = null;
            scores[metric] = 0;
        }
    }

    public void LoadHeroesFromPrefs()
    {
        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            string networkJson = PlayerPrefs.GetString(VHERO + metric.ToString(), null);
            if (networkJson == null) return;
            heroes[metric] = JsonUtility.FromJson<NeuralNetwork>(networkJson);
            var scoreStr = PlayerPrefs.GetString(VSCOR + metric.ToString(), null);
            if (scoreStr == "") continue;
            scores[metric] = float.Parse(scoreStr, CultureInfo.InvariantCulture);
        }
    }

    public static void AddHero(NeuralNetwork network, Metric metric, float score)
    {
        if (score < scores[metric]) return;

        heroes[metric] = network;
        scores[metric] = score;

        OnHighscoreChanged?.Invoke(metric, score);

        PlayerPrefs.SetString(VHERO + metric.ToString(), JsonUtility.ToJson(network));
        PlayerPrefs.SetString(VSCOR + metric.ToString(),
            scores[metric].ToString(CultureInfo.InvariantCulture));
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
            PlayerPrefs.SetString(VSCOR + metric.ToString(),
                scores[metric].ToString(CultureInfo.InvariantCulture));
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
            PlayerPrefs.SetString(VHERO + metric.ToString(), JsonUtility.ToJson(NeuralNetwork.NewRandom()));
            PlayerPrefs.SetInt(VSCOR + metric.ToString(), 0);
        }
    }

    public static NeuralNetwork GetRandomHero()
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
        return NeuralNetwork.NewRandom();
    }
}