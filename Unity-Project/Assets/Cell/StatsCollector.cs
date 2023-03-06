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

    public void AddToValhalla(NeuralNetwork nn)
    {
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