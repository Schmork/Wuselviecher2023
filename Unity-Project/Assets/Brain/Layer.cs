using System;

[Serializable]
public class Layer : ICloneable
{
    public ActivationFunction[] NeuronFunction;
    public float[] NeuronBias;
    public float[] PrevOutput;
    public float[] Weights;

    public Layer(int neuronCount, int inputSize, ActivationFunction? function = null)
    {
        NeuronFunction = new ActivationFunction[neuronCount];
        NeuronBias = new float[neuronCount];
        PrevOutput = new float[neuronCount];
        Weights = new float[neuronCount * inputSize];

        for (int i = 0; i < neuronCount; i++)
        {
            NeuronBias[i] = (float)WorldConfig.Random.NextDouble() * 2f - 1f;
            if (function == null)
            {
                NeuronFunction[i] = WorldConfig.Random.Next(2) == 0 ? ActivationFunction.Sigmoid : ActivationFunction.TanH;
            }
            else
            {
                NeuronFunction[i] = (ActivationFunction)function;
            }

            for (int j = 0; j < inputSize; j++)
            {
                Weights[i * inputSize + j] = (float)WorldConfig.Random.NextDouble() * 2f - 1f;
            }
        }
    }

    public Layer(float[] weights, float[] neuronBias, ActivationFunction[] neuronFunction)
    {
        NeuronFunction = neuronFunction;
        NeuronBias = neuronBias;
        PrevOutput = new float[neuronBias.Length];
        Weights = weights;
    }

    public float[] FeedForward(float[] input)
    {
        float[] output = new float[NeuronBias.Length];

        for (int i = 0; i < NeuronBias.Length; i++)
        {
            float sum = 0f;
            if (NeuronFunction[i] == ActivationFunction.Input)
            {
                output[i] = Activation.Evaluate(NeuronFunction[i], NeuronBias[i] * input[i]);
            }
            else
            {

                for (int j = 0; j < input.Length; j++)
                {
                    sum += Weights[i * input.Length + j] * input[j];
                }
                output[i] = Activation.Evaluate(NeuronFunction[i], NeuronBias[i] * sum);
            }
            PrevOutput[i] = output[i];
        }

        return output;
    }

    public object Clone()
    {
        var newWeights = (float[])Weights.Clone();
        var newNeurons = (float[])NeuronBias.Clone();
        var newFunctions = (ActivationFunction[])NeuronFunction.Clone();
        return new Layer(newWeights, newNeurons, newFunctions);
    }
}