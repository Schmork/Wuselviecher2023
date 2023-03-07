using UnityEngine;

public class StatsCollector : MonoBehaviour
{
    public float DistanceTravelled;
    public int NumEaten;
    public float MassEaten;
    public float TimeSpawned;
    public float MassEatenAtSpeed;
    public float FastestSpeedAchieved;
    public int ActionsTaken;
    public float StraightMass;
    public Valhalla Valhalla;
    MovementController mc;

    void Awake()
    {
        mc = GetComponent<MovementController>();
    }

    void OnEnable()
    {
        TimeSpawned = Time.time;
        DistanceTravelled = 0;
        NumEaten = 0;
        MassEaten = 0;
        MassEatenAtSpeed = 0;
        FastestSpeedAchieved = 0;
        ActionsTaken = 0;
        StraightMass = 0;
    }

    void OnDisable()
    {
        var nn = mc.Brain;
        if (nn == null) return;
        Valhalla.AddFallenHero(nn, DistanceTravelled, Valhalla.Metric.DistanceTravelled);
        Valhalla.AddFallenHero(nn, FastestSpeedAchieved, Valhalla.Metric.FastestSpeedAchieved);
        Valhalla.AddFallenHero(nn, MassEaten, Valhalla.Metric.MassEaten);
        Valhalla.AddFallenHero(nn, MassEatenAtSpeed, Valhalla.Metric.MassEatenAtSpeed);
        Valhalla.AddFallenHero(nn, NumEaten, Valhalla.Metric.NumEaten);
        Valhalla.AddFallenHero(nn, Time.time - TimeSpawned, Valhalla.Metric.TimeSurvived);
        Valhalla.AddFallenHero(nn, 30f * MassEaten / (ActionsTaken + 10f), Valhalla.Metric.MassPerAction);
        Valhalla.AddFallenHero(nn, StraightMass, Valhalla.Metric.StraightMass);
    }
}