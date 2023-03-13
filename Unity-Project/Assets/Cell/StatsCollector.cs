using System.Collections.Generic;
using UnityEngine;

public class StatsCollector : MonoBehaviour
{
    [SerializeField] SizeController sc;
    [SerializeField] MovementController mc;

    Dictionary<Valhalla.Metric, float> scores;

    int actionsTaken;
    public int ActionsTaken
    {
        get { return actionsTaken; }
        set
        {
            actionsTaken++;
            UpdateScore(Valhalla.Metric.MassPerAction, scores[Valhalla.Metric.MassEaten] / actionsTaken);
        }
    }

    public void AddToScore(Valhalla.Metric key, float value)
    {
        scores[key] += value;
        Valhalla.AddHero(mc.Brains, key, scores[key]);
    }

    public void UpdateScore(Valhalla.Metric key, float value)
    {
        if (value > scores[key])
        {
            Valhalla.AddHero(mc.Brains, key, value);
        }
        scores[key] = value;
    }

    void OnEnable()
    {
        scores = new Dictionary<Valhalla.Metric, float>();
        foreach (Valhalla.Metric metric in System.Enum.GetValues(typeof(Valhalla.Metric)))
        {
            scores.Add(metric, 0f);
        }
    }

    void Update()
    {
        AddToScore(Valhalla.Metric.TimeSurvived, Time.deltaTime * sc.Size / 100f);
    }
}