using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
[Serializable]
public class NeuralNetwork : ICloneable
{
    public List<Layer> Layers;
    public int generation;

    public static readonly int numInputs = 48;  // 4 self, + sensors + memory
    public int2[] Memory;                       // x = layer, y = neuron index

    enum MutationType
    {
        BIAS,
        WEIGHT,
        MEMORY,
        FUNCTION
    }

    readonly static Dictionary<MutationType, int> mutations = new Dictionary<MutationType, int>()
    {
        { MutationType.BIAS, 15 },
        { MutationType.WEIGHT, 6 },
        { MutationType.MEMORY, 1 },
        { MutationType.FUNCTION, 1 }
    };

    public static NeuralNetwork NewRandom()
    {
        var nn = new NeuralNetwork
        {
            Layers = new List<Layer>() { new Layer(numInputs, 0, ActivationFunction.Identity) }
        };
        nn.AddLayer(32);
        nn.AddLayer(12);
        nn.AddLayer(4);

        nn.Memory = new int2[numInputs - SensorController.numSensorValues - 4];
        for (int i = 0; i < nn.Memory.Length; i++)
            RandomMemory(nn, i);

        nn.generation = 0;
        return nn;
    }

    void AddLayer(int numNeurons, ActivationFunction? function = null)
    {
        Layers.Add(new Layer(numNeurons, Layers[^1].Biases.Length, function));
    }

    static void RandomMemory(NeuralNetwork nn, int i)
    {
        var l = Utility.Random.NextInt(nn.Layers.Count);
        int n;
        do n = Utility.Random.NextInt(nn.Layers[l].Biases.Length);
        while (l == 0 && n >= nn.Memory.Length);      // don't wire memory to itself
        nn.Memory[i] = new int2(l, n);
    }

    [BurstCompile]
    public NeuralNetwork Mutate(float mutation = 0.01f)
    {
        var clone = (NeuralNetwork)Clone();
        clone.generation++;

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

        (int layer, int item) = GetRandomElementIndex(clone.Layers, mutationType);
        switch (mutationType)
        {
            case MutationType.BIAS:
                clone.Layers[layer].Biases[item] += Utility.Gauss(mutation);
                break;
            case MutationType.WEIGHT:
                clone.Layers[layer].Weights[item] += Utility.Gauss(mutation);
                break;
            case MutationType.MEMORY:
                RandomMemory(clone, Utility.Random.NextInt(clone.Memory.Length));
                break;
            case MutationType.FUNCTION:
                clone.Layers[layer].Functions[item] = Layer.RandomFunction();
                break;
        }
        return clone;
    }

    static (int layer, int item) GetRandomElementIndex(List<Layer> layers, MutationType which)
    {
        int total = 0;
        for (int i = 0; i < layers.Count; i++)
        {
            total += which switch
            {
                MutationType.BIAS => layers[i].Biases.Length,
                MutationType.WEIGHT => layers[i].Weights.Length,
                MutationType.MEMORY => layers[i].Memory.Length,
                MutationType.FUNCTION => layers[i].Functions.Length,
                _ => throw new Exception("Invalid element type")
            };
        }

        int randomIndex = Utility.Random.NextInt(total);
        for (int i = 0; i < layers.Count; i++)
        {
            int count = which switch
            {
                MutationType.BIAS => layers[i].Biases.Length,
                MutationType.WEIGHT => layers[i].Weights.Length,
                MutationType.MEMORY => layers[i].Memory.Length,
                MutationType.FUNCTION => layers[i].Functions.Length,
                _ => throw new Exception("Invalid element type")
            };
            if (randomIndex < count) return (i, randomIndex);
            randomIndex -= count;
        }
        throw new Exception("This should never happen");
    }

    [BurstCompile]
    public float4[] FeedForward(float4[] input)
    {
        int i;
        var result = Layers[0].FeedForwardInput(input);
        for (i = 1; i < Layers.Count; i++)
            result = Layers[i].FeedForward(result);
        return result;
    }

    public object Clone()
    {
        var clone = new NeuralNetwork
        {
            Memory = Memory.Clone() as int2[],
            generation = generation,
            Layers = new List<Layer>()
        };
        for (int i = 0; i < Layers.Count; i++)
            clone.Layers.Add(Layers[i].Clone() as Layer);
        return clone;
    }
}