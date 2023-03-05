using SharpNeat.Genomes.Neat;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Valhalla", menuName = "Custom/Valhalla")]
public class Valhalla : ScriptableObject
{
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

    private Dictionary<Metric, NeatGenome> fallenHeroes;
    private Dictionary<Metric, float> bestScores;

    public const string FILE_EXTENTION_POPULATION = ".pop.xml";
    public const string FILE_EXTENTION_HERO = ".hero.xml";
    public const string FILE_EXTENTION_FACTORY = ".factory.xml";

    public enum FileType { Hero, Population, Factory }

    public void OnEnable()
    {
        InitDictionaries();
        //LoadFallenHeroesFromPrefs();
    }

    void InitDictionaries()
    {
        fallenHeroes = new Dictionary<Metric, NeatGenome>();
        bestScores = new Dictionary<Metric, float>();
        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            fallenHeroes[metric] = null;
            bestScores[metric] = 0;
        }
    }

    public void LoadFallenHeroesFromPrefs()
    {
        return;
        //ToDo: Update to NEAT
        foreach (Metric metric in System.Enum.GetValues(typeof(Metric)))
        {
            string networkJson = PlayerPrefs.GetString(metric.ToString(), null);
            if (networkJson == null) return;
            //fallenHeroes[metric] = JsonUtility.FromJson<NeuralNetwork>(networkJson);
            var scoreStr = PlayerPrefs.GetString("score " + metric.ToString(), null);
            if (scoreStr == "") continue;
            bestScores[metric] = float.Parse(scoreStr, CultureInfo.InvariantCulture);
        }
    }

    public void AddFallenHero(NeatGenome genome, float score, Metric metricType)
    {
        if (score <= bestScores[metricType] || genome == null) return;

        //fallenHeroes[metricType] = genome.GenomeFactory.CreateGenomeCopy(genome, genome.GenomeFactory.NextGenomeId(), genome.BirthGeneration);
        fallenHeroes[metricType] = genome.GenomeFactory.CreateGenomeCopy(genome, genome.Id, genome.BirthGeneration);
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

        WriteHero(metricType, genome, score, FileType.Hero);
        return;
        //ToDo: Update to NEAT
        //PlayerPrefs.SetString(metricType.ToString(), JsonUtility.ToJson(network));
        PlayerPrefs.SetString("score " + metricType.ToString(),
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
        return;
        //ToDo: Update to NEAT
        InitDictionaries();

        foreach (var metric in System.Enum.GetValues(typeof(Metric)))
        {
            PlayerPrefs.SetString(metric.ToString(), JsonUtility.ToJson(new NeuralNetwork()));
            PlayerPrefs.SetInt("score " + metric.ToString(), 0);
        }

        var cells = FindObjectsOfType<SizeController>();
        for (int i = 0; i < cells.Length; i++)
        {
            Destroy(cells[i].gameObject);
        }
    }

    public static float[] chance = new float[] { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f, 0.2f };

    public NeatGenome GetRandomHero()
    {
        var sum = 0f;
        foreach (var item in chance) sum += item;
        var rand = WorldConfig.Random.NextDouble() * sum;

        sum = 0;
        for (int i = 0; i < chance.Length; i++)
        {
            sum += chance[i];
            if (rand < sum) return fallenHeroes[(Metric)i];
        }

        var metrics = System.Enum.GetValues(typeof(Metric));
        Metric randomMetric = (Metric)metrics.GetValue(Random.Range(0, metrics.Length));
        return fallenHeroes[randomMetric];
    }

    string GetSaveFilePath(FileType fileType, string metricName = "")
    {
        string extention = fileType switch
        {
            FileType.Hero => FILE_EXTENTION_HERO,
            FileType.Population => FILE_EXTENTION_POPULATION,
            FileType.Factory => FILE_EXTENTION_FACTORY,
            _ => ".UnknownNeatFileType",
        };
        return Application.persistentDataPath + "/" + metricName + extention;
    }

    bool WriteHero(Metric metricType, NeatGenome hero, float score, FileType fileType)
    {
        XmlWriterSettings _xwSettings = new XmlWriterSettings
        {
            Indent = true
        };

        string filePath = GetSaveFilePath(fileType, metricType.ToString());

        DirectoryInfo dirInf = new DirectoryInfo(Application.persistentDataPath);
        if (!dirInf.Exists)
        {
            Debug.Log("ExperimentIO - Creating subdirectory");
            dirInf.Create();
        }

        try
        {
            using XmlWriter xw = XmlWriter.Create(filePath, _xwSettings);
            NeatGenomeXmlIO.WriteComplete(xw, hero, false, metricType, score);
            //Debug.Log("Successfully saved the genome of the '" + fileType.ToString() + "' for Wuselviecher to the location:\n" + filePath);
        }
        catch (System.Exception)
        {
            Debug.Log("Error saving the genomes of the '" + fileType.ToString() + "' for Wuselviecher to the location:\n" + filePath);
            return false;
        }
        return true;
    }

    struct NodeInfo
    {
        public string type;
        public int id;
    }

    struct ConInfo
    {
        public int id;
        public int src;
        public int tgt;
        public float wght;
    }

    public List<NeatGenome> ReadHeroes()
    {
        var genomeList = new List<NeatGenome>();
        var genomeFactory = WorldController.Factory;
        var path = Application.persistentDataPath;
        var files = Directory.GetFiles(path, "*.hero.xml");

        //Debug.Log("Trying to read " + files.Length + " files from " + path);
        var f = 0;
        foreach (var file in files)
        {
            f++;
            using var reader = XmlReader.Create(file);

            Metric metric = default;
            float score = -1;
            int networkId = -1;
            int birthGen = -1;
            float fitness = -1; 
            var neuronGeneList = new List<NeuronGene>();
            var conList = new List<ConnectionGene>();

            while (reader.Read())
            {
                if (reader.Name == "Metric")
                {
                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.Name == "Metric")
                        {
                            var str = reader.ReadContentAsString();
                            var metricKeys = System.Enum.GetNames(typeof(Metric));
                            var metricValues = System.Enum.GetValues(typeof(Metric));
                            for (int i = 0; i < metricKeys.Length; i++)
                            {
                                if (str == metricKeys[i]) metric = (Metric)metricValues.GetValue(i);
                            }
                        }
                    }
                }

                if (reader.Name == "Score")
                {
                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.Name == "Score")
                        {
                            score = reader.ReadContentAsFloat();
                            //Debug.Log("Score: " + score);
                        }
                    }
                }

                else if (reader.Name == "Network")
                {
                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.Name == "id")
                        {
                            networkId = reader.ReadContentAsInt();
                            //Debug.Log("Network ID: " + networkId);
                        }
                        if (reader.Name == "birthGen")
                        {
                            birthGen = reader.ReadContentAsInt();
                            //Debug.Log("Birth Gen: " + birthGen);
                        }
                        if (reader.Name == "fitness")
                        {
                            fitness = reader.ReadContentAsInt();
                            //Debug.Log("fitness: " + fitness);
                        }
                    }

                    reader.ReadToFollowing("Node");
                    do
                    {
                        if (!reader.HasAttributes) continue;
                        var info = new NodeInfo();

                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "type") info.type = reader.ReadContentAsString();
                            if (reader.Name == "id") info.id = reader.ReadContentAsInt();
                        }

                        SharpNeat.Network.NodeType type = SharpNeat.Network.NodeType.Bias;
                        switch (info.type)
                        {
                            case "bias":
                                type = SharpNeat.Network.NodeType.Bias;
                                break;
                            case "in":
                                type = SharpNeat.Network.NodeType.Input;
                                break;
                            case "out":
                                type = SharpNeat.Network.NodeType.Output;
                                break;
                        }
                        neuronGeneList.Add(new NeuronGene((uint)info.id, type, 0));

                    } while (reader.ReadToNextSibling("Node"));

                    reader.ReadToFollowing("Con");
                    do
                    {
                        if (!reader.HasAttributes) continue;
                        var info = new ConInfo();

                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "id") info.id = reader.ReadContentAsInt();
                            if (reader.Name == "src") info.src = reader.ReadContentAsInt();
                            if (reader.Name == "tgt") info.tgt = reader.ReadContentAsInt();
                            if (reader.Name == "wght") info.wght = reader.ReadContentAsFloat();

                        }
                        conList.Add(new ConnectionGene((uint)info.id, (uint)info.src, (uint)info.tgt, info.wght));

                    } while (reader.ReadToNextSibling("Con"));
                }

            }
            var inputs = neuronGeneList.Count(n => n.NodeType == SharpNeat.Network.NodeType.Input);
            var outputs = neuronGeneList.Count(n => n.NodeType == SharpNeat.Network.NodeType.Output);

            var genome = new NeatGenome(genomeFactory, (uint)networkId, (uint)birthGen, new NeuronGeneList(neuronGeneList), new ConnectionGeneList(conList), inputs, outputs, false);
            genomeList.Add(genome);
            fallenHeroes[metric] = genome;
            bestScores[metric] = score;
        }
        //Debug.Log("Read " + f + " files successfully.");

        return genomeList;
    }
}

