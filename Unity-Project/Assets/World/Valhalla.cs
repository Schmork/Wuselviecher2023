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
        NumEaten,
        MassEaten,
        TimeSurvived,
        MassEatenAtSpeed,
        FastestSpeedAchieved,
        MassPerAction,
        StraightMass
    }

    private Dictionary<Metric, NeuralNetwork> fallenHeroes;
    private Dictionary<Metric, float> bestScores;
    public static int OldestGen = 0;

    public void OnEnable()
    {
        InitDictionaries();
        LoadFallenHeroesFromPrefs();
    }

    void InitDictionaries()
    {
        fallenHeroes = new Dictionary<Metric, NeuralNetwork>();
        bestScores = new Dictionary<Metric, float>();
        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            fallenHeroes[metric] = null;
            bestScores[metric] = 0;
        }
    }

    public void LoadFallenHeroesFromPrefs()
    {
        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            string networkJson = PlayerPrefs.GetString(VHERO + metric.ToString(), null);
            if (networkJson == null) return;
            fallenHeroes[metric] = JsonUtility.FromJson<NeuralNetwork>(networkJson);
            var scoreStr = PlayerPrefs.GetString(VSCOR + metric.ToString(), null);
            if (scoreStr == "") continue;
            bestScores[metric] = float.Parse(scoreStr, CultureInfo.InvariantCulture);
        }
    }

    public void AddFallenHero(NeuralNetwork network, float score, Metric metricType)
    {
        if (score <= bestScores[metricType] || network == null) return;

        fallenHeroes[metricType] = network;
        bestScores[metricType] = score;

        switch (metricType)
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
            case Metric.NumEaten:
                Dashboard.UpdateNumEatenRecord((int)score);
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

        PlayerPrefs.SetString(VHERO + metricType.ToString(), JsonUtility.ToJson(network));
        PlayerPrefs.SetString(VSCOR + metricType.ToString(),
            bestScores[metricType].ToString(CultureInfo.InvariantCulture));
    }

    public void RefreshDashboard()
    {
        Dashboard.UpdateDistanceTravelledRecord(bestScores[Metric.DistanceTravelled]);
        Dashboard.UpdateFastestSpeedAchievedRecord(bestScores[Metric.FastestSpeedAchieved]);
        Dashboard.UpdateMassEatenAtSpeedRecord(bestScores[Metric.MassEatenAtSpeed]);
        Dashboard.UpdateMassEatenRecord(bestScores[Metric.MassEaten]);
        Dashboard.UpdateNumEatenRecord((int)bestScores[Metric.NumEaten]);
        Dashboard.UpdateTimeSurvivedRecord(bestScores[Metric.TimeSurvived]);
        Dashboard.UpdateMassPerAction(bestScores[Metric.MassPerAction]);
        Dashboard.UpdateStraightMass(bestScores[Metric.StraightMass]);
    }

    public void DecayScores()
    {
        for (int i = 0; i < bestScores.Count; i++)
        {
            bestScores[(Metric)i] *= (1 - Dashboard.GetDecay());
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

        InitDictionaries();

        foreach (var metric in System.Enum.GetValues(typeof(Metric)))
        {
            PlayerPrefs.SetString(VHERO + metric.ToString(), JsonUtility.ToJson(NeuralNetwork.NewRandom()));
            PlayerPrefs.SetInt(VSCOR + metric.ToString(), 0);
        }
    }

    public static float[] chance = new float[] { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f };

    public NeuralNetwork GetRandomHero()
    {
        int i;
        var sum = 0f;
        for (i = 0; i < chance.Length; i++)
        {
            sum += chance[i];
        }

        var rand = WorldConfig.Random.NextDouble() * sum;

        sum = 0;
        for (i = 0; i < chance.Length; i++)
        {
            sum += chance[i];
            if (rand < sum) return fallenHeroes[(Metric)i];
        }

        var metrics = System.Enum.GetValues(typeof(Metric));
        Metric randomMetric = (Metric)metrics.GetValue(Random.Range(0, metrics.Length));
        return fallenHeroes[randomMetric];
    }
}