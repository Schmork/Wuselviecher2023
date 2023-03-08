using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Random = UnityEngine.Random;

[System.Serializable]
public class NeuralNetwork : System.ICloneable
{
    public List<Layer> Layers;
    public int generation;

    public static NeuralNetwork NewRandom()
    {
        var nn = new NeuralNetwork();

        var numInputs = 4 + SensorController.numSensorValues;
        UnityEngine.Debug.Assert(numInputs % 4 == 0);
        var numOutputs = 4;
        UnityEngine.Debug.Assert(numOutputs % 4 == 0);
        var numHidden = 32;// (numInputs + numOutputs) / 2;
        UnityEngine.Debug.Assert(numHidden % 4 == 0);
        
        UnityEngine.Debug.Assert(numInputs % 4 == 0);
        //UnityEngine.Debug.Log(numInputs + ", " + numHidden + ", " + numOutputs);
        nn.Layers = new List<Layer>() { new Layer(numInputs, 0, ActivationFunction.Identity) };
        UnityEngine.Debug.Assert(nn.Layers[^1].NeuronBias.Length % 4 == 0, "NN Fac LH NB length: " + nn.Layers[^1].NeuronBias.Length);
        nn.AddLayer(numHidden);
        UnityEngine.Debug.Assert(nn.Layers[^1].NeuronBias.Length % 4 == 0, "NN Fac LO NB length: " + nn.Layers[^1].NeuronBias.Length);
        nn.AddLayer(numOutputs);

        nn.Layers[^1].NeuronFunctions[0] = ActivationFunction.TanH;
        nn.Layers[^1].NeuronFunctions[1] = ActivationFunction.Sigmoid;

        nn.generation = 0;
        return nn;
    }

    public NeuralNetwork() { }

    public NeuralNetwork(NeuralNetwork parent, float mutation = 0.01f)
    {
        Layers = new List<Layer>();
        for (int i = 0; i < parent.Layers.Count; i++)
        {
            //UnityEngine.Debug.Assert(parent.Layers[i].NeuronBias.Length % 4 == 0, "NN mut + " + i + " NB = " + parent.Layers[i].NeuronBias.Length);
            UnityEngine.Debug.Assert(parent.Layers[i].Weights.Length % 4 == 0, "NN mut + " + i + " W = " + parent.Layers[i].Weights.Length);

            Layers.Add(parent.Layers[i].Clone() as Layer);
        }
        Mutate(mutation);
        generation = parent.generation + 1;
    }

    private void AddLayer(int numNeurons, ActivationFunction? function = null)
    {
        UnityEngine.Debug.Assert(numNeurons % 4 == 0, "NN nN = " + numNeurons);
        UnityEngine.Debug.Assert(Layers[^1].NeuronBias.Length % 4 == 0, "NN NB length: " + Layers[^1].NeuronBias.Length);
        Layers.Add(new Layer(numNeurons, Layers[^1].NeuronBias.Length, function));
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
        clone.generation = generation;
        return clone;
    }
}