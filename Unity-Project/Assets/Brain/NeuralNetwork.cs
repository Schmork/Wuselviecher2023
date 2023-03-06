using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class NeuralNetwork : System.ICloneable
{
    static readonly int memNeurons = SensorController.numSensorValues / 2;
    public List<Layer> Layers;
    public int[] memNeuronLayer = new int[memNeurons];
    public int[] memNeuronIndex = new int[memNeurons];

    public NeuralNetwork()
    {
        var numInputs = 3 + memNeurons + SensorController.numSensorValues;
        var numOutputs = 2;

        Layers = new List<Layer>() { new Layer(numInputs, 0, ActivationFunction.Input) };
        /*
        var numHidden1 = (numInputs + numOutputs) * 2 / 3;
        var numHidden2 = numHidden1 / 2;
        AddLayer(numHidden1);
        AddLayer(numHidden2);
        */
        AddLayer((numInputs + numOutputs) / 2);
        AddLayer(numOutputs);

        Layers[^1].NeuronFunctions[0] = ActivationFunction.TanH;
        Layers[^1].NeuronFunctions[1] = ActivationFunction.Sigmoid;

        for (int i = 0; i < memNeuronLayer.Length; i++)
        {
            RandomMemory(i);
        }
    }

    private void AddLayer(int numNeurons, ActivationFunction? function = null)
    {
        Layers.Add(new Layer(numNeurons, Layers[^1].NeuronBias.Length, function));
    }

    public NeuralNetwork(NeuralNetwork parent, float mutation = 0.01f)
    {
        Layers = new List<Layer>();
        foreach (var parentLayer in parent.Layers)
        {
            Layers.Add(parentLayer.Clone() as Layer);
        }
        memNeuronLayer = parent.memNeuronLayer.Clone() as int[];
        memNeuronIndex = parent.memNeuronIndex.Clone() as int[];

        Mutate(mutation);
    }

    enum MutationType
    {
        BIAS,
        WEIGHT,
        FUNCTION,
        MEMORY
    }

    readonly Dictionary<MutationType, int> mutations = new Dictionary<MutationType, int>()
    {
        { MutationType.BIAS, 10 },
        { MutationType.WEIGHT, 40 },
        { MutationType.FUNCTION, 1 },
        { MutationType.MEMORY, 3 }
    };

    void Mutate(float mutation)
    {
        var totalWeight = mutations.Values.Sum();
        var random = (int)Random.value * totalWeight;
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
                layer = Layers[Random.Range(0, Layers.Count)];
                i = Random.Range(0, layer.NeuronBias.Length);
                layer.NeuronBias[i] += Random.Range(-mutation, mutation);
                break;
            case MutationType.WEIGHT:
                layer = Layers[Random.Range(1, Layers.Count)];
                i = Random.Range(0, layer.Weights.Length);
                layer.Weights[i] += Random.Range(-mutation, mutation);
                break;
            case MutationType.FUNCTION:
                layer = Layers[Random.Range(1, Layers.Count - 1)];
                i = Random.Range(0, layer.NeuronFunctions.Length);
                layer.NeuronFunctions[i] = Layer.RandomFunction();
                break;
            case MutationType.MEMORY:
                i = Random.Range(0, memNeuronIndex.Length);
                RandomMemory(i);
                break;
            default:
                break;
        }
    }

    void RandomMemory(int i)
    {
        var layer = WorldConfig.Random.Next(1, Layers.Count);
        var index = WorldConfig.Random.Next(0, Layers[layer].NeuronBias.Length);
        memNeuronLayer[i] = layer;
        memNeuronIndex[i] = index;
    }

    public float[] FeedForward(float[] input)
    {
        var result = input;
        foreach (var layer in Layers)
        {
            result = layer.FeedForward(result);
        }
        return result;
    }

    public object Clone()
    {
        NeuralNetwork clone = new NeuralNetwork();
        foreach (var layer in Layers)
        {
            clone.Layers.Add(layer.Clone() as Layer);
        }
        return clone;
    }
}
