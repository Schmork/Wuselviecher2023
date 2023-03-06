﻿using System;

[Serializable]
public class Layer : ICloneable
{
    public ActivationFunction[] NeuronFunctions;
    public float[] NeuronBias;
    public float[] PrevOutput;
    public float[] Weights;
    static readonly float initial = 30f;

    public static ActivationFunction RandomFunction()
    {
        var functions = Enum.GetValues(typeof(ActivationFunction)) as ActivationFunction[];
        var randomIndex = WorldConfig.Random.Next(functions.Length - 1);
        return functions[randomIndex + 1];
    }

    static float RandomInitialValue()
    {
        return (float)WorldConfig.Random.NextDouble() * initial - initial / 2f;
    }

    public Layer(int neuronCount, int inputLength, ActivationFunction? function = null)
    {
        NeuronFunctions = new ActivationFunction[neuronCount];
        NeuronBias = new float[neuronCount];
        PrevOutput = new float[neuronCount];
        Weights = new float[neuronCount * inputLength];
        //UnityEngine.Debug.Log("new Layer (" + neuronCount + ", " + inputLength + "), " + Weights.Length + " weights");

        for (int i = 0; i < neuronCount; i++)
        {
            NeuronBias[i] = RandomInitialValue();
            NeuronFunctions[i] = function == null ? RandomFunction() : (ActivationFunction)function;

            for (int j = 0; j < inputLength; j++)
            {
                //UnityEngine.Debug.Log("i, j: " + i + ", " + j + ", i * neuronCount + j = " + (i * inputLength + j) + ", max: " + Weights.Length);
                Weights[i * inputLength + j] = RandomInitialValue();
            }
        }
    }

    public Layer(float[] weights, float[] neuronBias, ActivationFunction[] neuronFunction)
    {
        NeuronFunctions = neuronFunction;
        NeuronBias = neuronBias;
        PrevOutput = new float[neuronBias.Length];
        Weights = weights;
    }

    public object Clone()
    {
        var newWeights = Weights.Clone() as float[];
        var newNeurons = NeuronBias.Clone() as float[];
        var newFunctions = NeuronFunctions.Clone() as ActivationFunction[];
        return new Layer(newWeights, newNeurons, newFunctions);
    }

    public float[] FeedForward(float[] input)
    {
        float[] output = new float[NeuronBias.Length];

        for (int i = 0; i < NeuronBias.Length; i++)
        {
            PrevOutput[i] = output[i];

            float sum = 0f;
            if (NeuronFunctions[i] == ActivationFunction.Input)
            {
                output[i] = Activation.Evaluate(NeuronFunctions[i], NeuronBias[i] * input[i]);
            }
            else
            {
                for (int j = 0; j < input.Length; j++)
                {
                    var x = i * input.Length + j;
                    if (x >= Weights.Length)
                        UnityEngine.Debug.Log("i, j: " + i + ", " + j + ", i * input.Length + j = " + x + ", max: " + Weights.Length);
                    sum += Weights[x] * input[j];
                }
                output[i] = Activation.Evaluate(NeuronFunctions[i], NeuronBias[i] * sum);
                //if (float.IsNaN(output[i])) UnityEngine.Debug.Log(NeuronFunctions[i]);
            }
        }

        return output;
    }
}