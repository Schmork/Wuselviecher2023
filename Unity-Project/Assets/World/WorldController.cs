using SharpNeat.Decoders.Neat;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;
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

    public static NeatGenomeFactory Factory { get; set; }
    NeatGenomeDecoder decoder;

    void Start()
    {
        //var conList = new ConnectionList(30);
        //var nodeList = new NodeList(20);
        //var afLib = DefaultActivationFunctionLibrary.CreateLibraryCppn();
        //var netDef = new NetworkDefinition(15, 2, afLib, nodeList, conList);
        var neatGenomeParameters = new NeatGenomeParameters
        {
            FeedforwardOnly = false,
            ConnectionWeightRange = 3,
            AddConnectionMutationProbability = 0.1,
            AddNodeMutationProbability = 0.05,
            InitialInterconnectionsProportion = 0.5,
            ConnectionWeightMutationProbability = 0.01
        };
        Factory = new NeatGenomeFactory(15, 2, neatGenomeParameters);
        Factory.GenomeIdGenerator.Reset();
        Factory.InnovationIdGenerator.Reset();
        decoder = new NeatGenomeDecoder(SharpNeat.Decoders.NetworkActivationScheme.CreateAcyclicScheme());

        spans = new System.Collections.Generic.List<long>() { 100 };
        //Valhalla.ReadHeroes();
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
        var existingCells = FindObjectsOfType<SizeController>();
        while (n < 4 && existingCells.Sum(cell => cell.GetComponent<SizeController>().Size) < WorldMinMass)
        {
            var pos = RandomFreePosition(existingCells);
            if (pos == Vector2.zero)
            {
                n++;
                continue;
            }
            var randomZRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

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

            NeatGenome genome;
            var hero = Valhalla.GetRandomHero();
            if (hero == null)
            {
                genome = Factory.CreateGenomeList(1, 0)[0];
                //Debug.Log("Spawned ID " + genome.Id);
            }
            else
            {
                //Debug.Log("Mutating genome");
                var gen = hero.BirthGeneration;
                genome = hero.CreateOffspring(gen);
                //Debug.Log("Spawned ID " + genome.Id);
            }
            var network = decoder.Decode(genome);
            //Debug.Log("Decoded genome, n = " + n);
            genome.EvaluationInfo.AuxFitnessArr = new SharpNeat.Core.AuxFitnessInfo[8];
            var cc = cell.GetComponent<CellController>();
            cc.SetGenome(genome);
            cc.ActivateUnit(network);
            n++;
        }
    }

    Vector2 RandomFreePosition(SizeController[] others)
    {
        Vector2 pos;
        int maxAttempts = 20;

        do
        {
            pos = (Vector2)transform.position + Random.insideUnitCircle * CellSpawnRadius;
            maxAttempts--;
        } while (maxAttempts > 0 && others.Any(o => Vector2.Distance(o.transform.position, pos) < o.Size2Scale()));

        return maxAttempts > 0 ? pos : Vector2.zero;
    }
}