using System.Collections.Generic;
using System.Globalization;
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
        FastestSpeedAchieved,
        MassPerAction,
        StraightMass
    }

    private static Dictionary<Metric, NeuralNetwork> heroes;
    private static Dictionary<Metric, float> scores;
    public static int OldestGen = 0;

    public void OnEnable()
    {
        InitDictionaries();
        LoadHeroesFromPrefs();
    }

    void InitDictionaries()
    {
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

        switch (metric)
        {
            case Metric.DistanceTravelled:
                Dashboard.UpdateDistanceTravelledRecord(score);
                break;
            case Metric.FastestSpeedAchieved:
                Dashboard.UpdateFastestSpeedAchievedRecord(score);
                break;
            case Metric.MassEaten:
                Dashboard.UpdateMassEatenRecord(score);
                break;
            case Metric.MassEatenAtSpeed:
                Dashboard.UpdateMassEatenAtSpeedRecord(score);
                break;
            case Metric.TimeSurvived:
                Dashboard.UpdateTimeSurvivedRecord(score);
                break;
            case Metric.MassPerAction:
                Dashboard.UpdateMassPerAction(score);
                break;
            case Metric.StraightMass:
                Dashboard.UpdateStraightMass(score);
                break;
        }

        PlayerPrefs.SetString(VHERO + metric.ToString(), JsonUtility.ToJson(network));
        PlayerPrefs.SetString(VSCOR + metric.ToString(),
            scores[metric].ToString(CultureInfo.InvariantCulture));
    }

    public void RefreshDashboard()
    {
        Dashboard.UpdateDistanceTravelledRecord(scores[Metric.DistanceTravelled]);
        Dashboard.UpdateFastestSpeedAchievedRecord(scores[Metric.FastestSpeedAchieved]);
        Dashboard.UpdateMassEatenAtSpeedRecord(scores[Metric.MassEatenAtSpeed]);
        Dashboard.UpdateMassEatenRecord(scores[Metric.MassEaten]);
        Dashboard.UpdateTimeSurvivedRecord(scores[Metric.TimeSurvived]);
        Dashboard.UpdateMassPerAction(scores[Metric.MassPerAction]);
        Dashboard.UpdateStraightMass(scores[Metric.StraightMass]);
        Dashboard.UpdateCellMaxGen(OldestGen);
    }

    public void DecayScores()
    {
        for (int i = 0; i < scores.Count; i++)
        {
            scores[(Metric)i] *= (1 - Dashboard.Decay);
        }
        RefreshDashboard();
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
        RefreshDashboard();

        foreach (var metric in System.Enum.GetValues(typeof(Metric)))
        {
            PlayerPrefs.SetString(VHERO + metric.ToString(), JsonUtility.ToJson(NeuralNetwork.NewRandom()));
            PlayerPrefs.SetInt(VSCOR + metric.ToString(), 0);
        }
    }

    public static float[] chance = new float[] { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f };

    public NeuralNetwork GetRandomHero()
    {
        int i;
        var sum = 0f;
        for (i = 0; i < chance.Length; i++)
        {
            sum += chance[i];
        }

        var rand = Utility.Random.NextDouble() * sum;

        sum = 0;
        for (i = 0; i < chance.Length; i++)
        {
            sum += chance[i];
            if (rand < sum) return heroes[(Metric)i];
        }

        var metrics = System.Enum.GetValues(typeof(Metric));
        Metric randomMetric = (Metric)metrics.GetValue(Random.Range(0, metrics.Length));
        return heroes[randomMetric];
    }
}