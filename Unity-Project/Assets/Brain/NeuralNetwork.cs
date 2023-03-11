using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
[System.Serializable]
public class NeuralNetwork : System.ICloneable
{
    public List<Layer> Layers;
    public int generation;

    public static readonly int numInputs = 48;   // 4 self, + sensors + memory
    public int2[] Memory = new int2[numInputs - SensorController.numSensorValues - 4];     // x = layer, y = neuron index

    public static NeuralNetwork NewRandom()
    {
        var nn = new NeuralNetwork
        {
            Layers = new List<Layer>() { new Layer(numInputs, 0, ActivationFunction.Identity) }
        };
        nn.AddLayer(32);
        nn.AddLayer(12);
        nn.AddLayer(4);

        nn.generation = 0;

        for (int i = 0; i < nn.Memory.Length; i++)
        {
            RandomMemory(nn, i);
        }

        return nn;
    }

    NeuralNetwork() { }

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
        var l = Utility.Random.NextInt(nn.Layers.Count);
        int n;
        do
        {
            n = Utility.Random.NextInt(nn.Layers[l].NeuronBias.Length);
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

    [BurstCompile]
    void Mutate(float mutation)
    {
        var totalWeight = mutations.Values.Sum();
        var random = Utility.Random.NextFloat(totalWeight);
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
                    layer = Layers[Utility.Random.NextInt(Layers.Count)];
                    i = Utility.Random.NextInt(layer.NeuronBias.Length);
                    layer.NeuronBias[i] += Utility.Gauss(mutation);
                }
                break;
            case MutationType.WEIGHT:
                layer = Layers[1 + Utility.Random.NextInt(Layers.Count - 1)];
                i = Utility.Random.NextInt(layer.Weights.Length);
                layer.Weights[i] += Utility.Gauss(mutation);
                break;
            case MutationType.MEMORY:
                i = Utility.Random.NextInt(Memory.Length);
                RandomMemory(this, i);
                break;
            case MutationType.FUNCTION:
                layer = Layers[1 + Utility.Random.NextInt(Layers.Count - 1)];
                i = Utility.Random.NextInt(layer.NeuronFunctions.Length);
                layer.NeuronFunctions[i] = Layer.RandomFunction();
                break;
            default:
                break;
        }
    }

    [BurstCompile]
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