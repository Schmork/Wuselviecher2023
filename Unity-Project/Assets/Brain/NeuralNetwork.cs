using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Random = UnityEngine.Random;

[System.Serializable]
public class NeuralNetwork : System.ICloneable
{
    public List<Layer> Layers;
    public int generation;

    public static readonly int numInputs = 64;   // 4 self, + sensors + memory
    public int2[] Memory = new int2[numInputs - SensorController.numSensorValues - 4];     // x = layer, y = neuron index

    public static NeuralNetwork NewRandom()
    {
        var nn = new NeuralNetwork();

        var numOutputs = 4;
        var numHidden = 32;// (numInputs + numOutputs) / 2;

        nn.Layers = new List<Layer>() { new Layer(numInputs, 0, ActivationFunction.Identity) };
        nn.AddLayer(numHidden);
        nn.AddLayer(numOutputs);

        nn.generation = 0;

        for (int i = 0; i < nn.Memory.Length; i++)
        {
            RandomMemory(nn, i);
        }

        return nn;
    }

    public NeuralNetwork() { }

    public NeuralNetwork(NeuralNetwork parent, float mutation = 0.01f)
    {
        Layers = new List<Layer>();
        for (int i = 0; i < parent.Layers.Count; i++)
        {
            Layers.Add(parent.Layers[i].Clone() as Layer);
        }
        Mutate(mutation);
        generation = parent.generation + 1;
    }

    private void AddLayer(int numNeurons, ActivationFunction? function = null)
    {
        Layers.Add(new Layer(numNeurons, Layers[^1].NeuronBias.Length, function));
    }

    private static void RandomMemory(NeuralNetwork nn, int i)
    {
        var l = Random.Range(0, nn.Layers.Count);
        int n;
        do
        {
            n = Random.Range(0, nn.Layers[l].NeuronBias.Length);
        } while (l == 0 && n >= nn.Memory.Length);      // don't wire memory to itself
        nn.Memory[i] = new int2(l, n);
    }

    enum MutationType
    {
        BIAS,
        WEIGHT,
        MEMORY,
        FUNCTION
    }

    readonly Dictionary<MutationType, int> mutations = new Dictionary<MutationType, int>()
    {
        { MutationType.BIAS, 15 },
        { MutationType.WEIGHT, 6 },
        { MutationType.MEMORY, 1 },
        { MutationType.FUNCTION, 1 }
    };

    void Mutate(float mutation)
    {
        var totalWeight = mutations.Values.Sum();
        var random = Random.value * totalWeight;
        var mutationType = MutationType.WEIGHT;
        var sum = 0;
        foreach (var mut in mutations)
        {
            sum += mut.Value;
            if (sum > random)
            {
                mutationType = mut.Key;
                break;
            }
        }

        int i;
        Layer layer;
        switch (mutationType)
        {
            case MutationType.BIAS:
                for (int n = 0; n < 2; n++)
                {
                    layer = Layers[Random.Range(0, Layers.Count)];
                    i = Random.Range(0, layer.NeuronBias.Length);
                    layer.NeuronBias[i] += Utility.Gauss(mutation);
                }
                break;
            case MutationType.WEIGHT:
                layer = Layers[Random.Range(1, Layers.Count)];
                i = Random.Range(0, layer.Weights.Length);
                layer.Weights[i] += Utility.Gauss(mutation);
                break;
            case MutationType.MEMORY:
                i = Random.Range(0, Memory.Length);
                RandomMemory(this, i);
                break;
            case MutationType.FUNCTION:
                layer = Layers[Random.Range(1, Layers.Count)];
                i = Random.Range(0, layer.NeuronFunctions.Length);
                layer.NeuronFunctions[i] = Layer.RandomFunction();
                break;
            default:
                break;
        }
    }

    public float4[] FeedForward(float4[] input)
    {
        int i;
        var result = Layers[0].FeedForwardInput(input);
        for (i = 1; i < Layers.Count; i++)
        {
            result = Layers[i].FeedForward(result);
        }
        return result;
    }

    public object Clone()
    {
        NeuralNetwork clone = new NeuralNetwork();
        for (int i = 0; i < Layers.Count; i++)
        {
            clone.Layers.Add(Layers[i].Clone() as Layer);
        }
        clone.Memory = Memory.Clone() as int2[];
        clone.generation = generation;
        return clone;
    }
}