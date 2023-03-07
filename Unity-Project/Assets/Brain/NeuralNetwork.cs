using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class NeuralNetwork : System.ICloneable
{
    public List<Layer> Layers;
    public int generation;

    public NeuralNetwork()
    {
        var numInputs = 4 + SensorController.numSensorValues;
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

        generation = 0;
    }

    private void AddLayer(int numNeurons, ActivationFunction? function = null)
    {
        Layers.Add(new Layer(numNeurons, Layers[^1].NeuronBias.Length, function));
    }

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

    enum MutationType
    {
        BIAS,
        WEIGHT,
        FUNCTION
    }

    readonly Dictionary<MutationType, int> mutations = new Dictionary<MutationType, int>()
    {
        { MutationType.BIAS, 10 },
        { MutationType.WEIGHT, 40 },
        { MutationType.FUNCTION, 1 }
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
            default:
                break;
        }
    }

    public float[] FeedForward(float[] input)
    {
        int i;
        var result = input;
        for (i = 0; i < Layers.Count; i++)
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
        return clone;
    }
}
