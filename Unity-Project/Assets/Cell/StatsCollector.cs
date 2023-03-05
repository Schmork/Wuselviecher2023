using UnityEngine;
using SharpNeat.Core;
using SharpNeat.Genomes.Neat;

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

    public void AddToValhalla(NeatGenome genome)
    {
        Valhalla.AddFallenHero(genome, DistanceTravelled, Valhalla.Metric.DistanceTravelled);
        Valhalla.AddFallenHero(genome, FastestSpeedAchieved, Valhalla.Metric.FastestSpeedAchieved);
        Valhalla.AddFallenHero(genome, MassEaten, Valhalla.Metric.MassEaten);
        Valhalla.AddFallenHero(genome, MassEatenAtSpeed, Valhalla.Metric.MassEatenAtSpeed);
        Valhalla.AddFallenHero(genome, NumEaten, Valhalla.Metric.NumEaten);
        Valhalla.AddFallenHero(genome, Time.time - TimeSpawned, Valhalla.Metric.TimeSurvived);
        Valhalla.AddFallenHero(genome, 100f * MassEaten / (ActionsTaken + 10f), Valhalla.Metric.MassPerAction);
        Valhalla.AddFallenHero(genome, StraightMass, Valhalla.Metric.StraightMass);
    }

    public void EvaluateGenomeFitness(SharpNeat.Genomes.Neat.NeatGenome genome)
    {
        var fitness = genome.EvaluationInfo.AuxFitnessArr;
        var i = 0;
        fitness[i++] = new AuxFitnessInfo(Valhalla.Metric.DistanceTravelled.ToString(), DistanceTravelled);
        fitness[i++] = new AuxFitnessInfo("FastestSpeed", FastestSpeedAchieved);
        fitness[i++] = new AuxFitnessInfo("MassEatenAtSpeed", MassEatenAtSpeed);
        fitness[i++] = new AuxFitnessInfo("MassEaten", MassEaten);
        fitness[i++] = new AuxFitnessInfo("NumEaten", NumEaten);
        fitness[i++] = new AuxFitnessInfo("TimeSurvived", Time.time - TimeSpawned);
        fitness[i++] = new AuxFitnessInfo("MassPerAction", MassEaten / (ActionsTaken + 10f));
        fitness[i++] = new AuxFitnessInfo("StraightMass", StraightMass);
    }
}