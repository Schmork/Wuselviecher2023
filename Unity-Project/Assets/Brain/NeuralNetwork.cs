using System;
using System.Collections.Generic;

[Serializable]
public class NeuralNetwork : ICloneable
{
    static readonly int memNeurons = 5;
    public List<Layer> Layers;
    public int[] memNeuronLayer = new int[memNeurons];
    public int[] memNeuronIndex = new int[memNeurons];

    public NeuralNetwork()
    {
        Layers = new List<Layer>() { new Layer(20, 0, ActivationFunction.Input) };
        AddLayer(11);
        AddLayer(2);

        Layers[^1].NeuronFunction[0] = ActivationFunction.TanH;
        Layers[^1].NeuronFunction[1] = ActivationFunction.Sigmoid;

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
        foreach (var layer in parent.Layers)
        {
            var newLayer = new Layer(layer.Weights, layer.NeuronBias, layer.NeuronFunction);
            var inputSize = layer.Weights.Length / layer.NeuronBias.Length;
            for (int i = 0; i < newLayer.NeuronBias.Length; i++)
            {
                newLayer.NeuronBias[i] += UnityEngine.Random.Range(-mutation, mutation);
                for (int j = 0; j < inputSize; j++)
                {
                    newLayer.Weights[i * inputSize + j] += UnityEngine.Random.Range(-mutation, mutation);
                }
            }
            Layers.Add(newLayer);
        }
        memNeuronLayer = (int[])parent.memNeuronLayer.Clone();
        memNeuronIndex = (int[])parent.memNeuronIndex.Clone();

        for (int i = 0; i < memNeuronLayer.Length; i++)
        {
            if (WorldConfig.Random.NextDouble() < mutation)
                RandomMemory(i);
        }
    }

    void RandomMemory(int i)
    {
        var layer = WorldConfig.Random.Next(1, Layers.Count - 2);   // avoid input layer due to funky values
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
