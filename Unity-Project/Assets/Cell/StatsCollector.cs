using System.Collections.Generic;
using UnityEngine;

public class StatsCollector : MonoBehaviour
{
    NeuralNetwork NeuralNetwork;

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
        Valhalla.AddHero(NeuralNetwork, key, scores[key]);
    }

    public void UpdateScore(Valhalla.Metric key, float value)
    {
        if (value > scores[key])
        {
            Valhalla.AddHero(NeuralNetwork, key, value);
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
        NeuralNetwork = GetComponent<MovementController>().Brain;
    }

    void Update()
    {
        AddToScore(Valhalla.Metric.TimeSurvived, Time.deltaTime);
    }
}