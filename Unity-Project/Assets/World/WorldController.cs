using SharpNeat.Decoders.Neat;
using SharpNeat.Genomes.Neat;
using System.Linq;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    [SerializeField] Valhalla Valhalla;
    [SerializeField] float ValhallaMutation;
    [SerializeField] Vector2 DecayScoreTimes;
    [SerializeField] int WorldMinMass;
    
    [SerializeField] GameObject CellPrefab;
    [SerializeField] Vector2 CellSpawnTimes;
    [SerializeField] float CellSpawnRadius;

    public static System.Collections.Generic.List<long> spans;

    NeatGenomeFactory factory;
    NeatGenomeDecoder decoder;

    void Start()
    {
        factory = new NeatGenomeFactory(15, 2);
        factory.NeatGenomeParameters.FeedforwardOnly = true;
        decoder = new NeatGenomeDecoder(SharpNeat.Decoders.NetworkActivationScheme.CreateAcyclicScheme());

        spans = new System.Collections.Generic.List<long>() { 100 };
        Valhalla.RefreshDashboard();
        InvokeRepeating(nameof(Spawn), CellSpawnTimes.x, CellSpawnTimes.y);
        InvokeRepeating(nameof(DecayScores), DecayScoreTimes.x, DecayScoreTimes.y);
    }

    void DecayScores()
    {
        Valhalla.DecayScores();
    }

    void Spawn()
    {
        //Debug.Log(spans.Average());

        int n = 0;
        var existingCells = FindObjectsOfType<MovementController>();
        while (n++ < 7 && existingCells.Sum(cell => cell.GetComponent<SizeController>().Size) < WorldMinMass)
        {
            var pos = transform.position + (Vector3)Random.insideUnitCircle * CellSpawnRadius;
            var randomZRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            foreach (var other in existingCells)
            {
                if (Vector2.Distance(other.transform.position, pos) < other.GetComponent<SizeController>().Size2Scale())
                    return;
            }

            var cell = Instantiate(CellPrefab, pos, randomZRotation, transform);
            cell.GetComponent<SpriteRenderer>().material.color =
                Random.ColorHSV(
                0, 1,
                0.6f, 1,
                0.6f, 1,
                1, 1);
            cell.GetComponent<SizeController>().Size = Random.Range(
                WorldConfig.Instance.CellSizeMin,
                WorldConfig.Instance.CellSizeMax);
            cell.GetComponent<StatsCollector>().Valhalla = Valhalla;

            //var hero = Valhalla.GetRandomHero();

            factory.GenomeIdGenerator.Reset();
            var network = decoder.Decode(factory.CreateGenomeList(1, 0)[0]);
            cell.GetComponent<CellController>().ActivateUnit(network);
        }
    }
}