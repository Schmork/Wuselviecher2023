using System;

[Serializable]
public class Layer : ICloneable
{
    public ActivationFunction[] NeuronFunctions;
    public float[] NeuronBias;
    public float[] Weights;
    static readonly float initial = 30f;

    public static ActivationFunction RandomFunction()
    {
        var functions = Enum.GetValues(typeof(ActivationFunction)) as ActivationFunction[];
        var randomIndex = 1 + WorldConfig.Random.Next(functions.Length - 1); // skip "INPUT"
        return functions[randomIndex];
    }

    static float RandomInitialValue()
    {
        return (float)WorldConfig.Random.NextDouble() * initial - initial / 2f;
    }

    public Layer(int neuronCount, int inputLength, ActivationFunction? function = null)
    {
        NeuronFunctions = new ActivationFunction[neuronCount];
        NeuronBias = new float[neuronCount];
        Weights = new float[neuronCount * inputLength];

        for (int i = 0; i < neuronCount; i++)
        {
            NeuronBias[i] = RandomInitialValue();
            NeuronFunctions[i] = function == null ? RandomFunction() : (ActivationFunction)function;

            for (int j = 0; j < inputLength; j++)
            {
                Weights[i * inputLength + j] = RandomInitialValue();
            }
        }
    }

    public Layer(float[] weights, float[] neuronBias, ActivationFunction[] neuronFunction)
    {
        NeuronFunctions = neuronFunction;
        NeuronBias = neuronBias;
        Weights = weights;
    }

    public object Clone()
    {
        return new Layer(
            Weights.Clone() as float[], 
            NeuronBias.Clone() as float[], 
            NeuronFunctions.Clone() as ActivationFunction[]
            );
    }

    public float[] FeedForward(float[] input)
    {
        float[] output = new float[NeuronBias.Length];

        float sum;
        for (int i = 0; i < NeuronBias.Length; i++)
        {
            sum = 0f;
            if (NeuronFunctions[i] == ActivationFunction.Input)
            {
                output[i] = NeuronBias[i] * input[i];
            }
            else
            {
                for (int j = 0; j < input.Length; j++)
                {
                    sum += Weights[i * input.Length + j] * input[j];
                }
                output[i] = Activation.Evaluate(NeuronFunctions[i], NeuronBias[i] * sum);
            }
        }

        return output;
    }
}