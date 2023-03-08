using Unity.Burst;
using System;
using Unity.Mathematics;

[BurstCompile]
[Serializable]
public class Layer : ICloneable
{
    public ActivationFunction[] NeuronFunctions;
    public float4[] NeuronBias;
    public float4[] Weights;

    public Layer(int neuronCount, int inputLength, ActivationFunction? function = null)
    {
        NeuronFunctions = new ActivationFunction[neuronCount];
        NeuronBias = new float4[neuronCount / 4];
        Weights = new float4[neuronCount * inputLength / 4];

        int i;
        for (i = 0; i < NeuronBias.Length; i++)
        {
            NeuronBias[i] = RandomInitialValue();
            NeuronFunctions[i + 0] = function == null ? RandomFunction() : (ActivationFunction)function;
            NeuronFunctions[i + 1] = function == null ? RandomFunction() : (ActivationFunction)function;
            NeuronFunctions[i + 2] = function == null ? RandomFunction() : (ActivationFunction)function;
            NeuronFunctions[i + 3] = function == null ? RandomFunction() : (ActivationFunction)function;
        }
        for (i = 0; i < Weights.Length; i++)
        {
            Weights[i] = RandomInitialValue();
        }
    }

    public Layer(float4[] weights, float4[] neuronBias, ActivationFunction[] neuronFunction)
    {
        NeuronFunctions = neuronFunction;
        NeuronBias = neuronBias;
        Weights = weights;
    }

    public static ActivationFunction RandomFunction()
    {
        var functions = Enum.GetValues(typeof(ActivationFunction)) as ActivationFunction[];
        var randomIndex = 1 + WorldConfig.Random.Next(functions.Length - 1); // skip "INPUT"
        return functions[randomIndex];
    }

    static float4 RandomInitialValue()
    {
        var initial = WorldConfig.Instance.InitialValues;
        return new float4(
            (float)WorldConfig.Random.NextDouble() * initial - initial / 2f,
            (float)WorldConfig.Random.NextDouble() * initial - initial / 2f,
            (float)WorldConfig.Random.NextDouble() * initial - initial / 2f,
            (float)WorldConfig.Random.NextDouble() * initial - initial / 2f
            );
    }

    public object Clone()
    {
        return new Layer(
            Weights.Clone() as float4[],
            NeuronBias.Clone() as float4[],
            NeuronFunctions.Clone() as ActivationFunction[]
            );
    }

    [BurstCompile]
    public float4[] FeedForward(float4[] input)
    {
        float4[] output = new float4[NeuronBias.Length];

        float sum;
        for (int i = 0; i < NeuronBias.Length; i++)
        {
            sum = 0f;
            for (int j = 0; j < input.Length; j++)
            {
                sum += math.dot(Weights[i * input.Length + j], input[j]);
            }
            output[i].w = Activation.Evaluate(NeuronFunctions[i + 0], NeuronBias[i].w * sum);
            output[i].x = Activation.Evaluate(NeuronFunctions[i + 1], NeuronBias[i].x * sum);
            output[i].y = Activation.Evaluate(NeuronFunctions[i + 2], NeuronBias[i].y * sum);
            output[i].z = Activation.Evaluate(NeuronFunctions[i + 3], NeuronBias[i].z * sum);
        }

        return output;
    }

    [BurstCompile]
    public float4[] FeedForwardInput(float4[] input)
    {
        float4[] output = new float4[NeuronBias.Length];
        for (int i = 0; i < NeuronBias.Length; i++)
        {
            output[i] = NeuronBias[i] * input[i];
        }
        return output;
    }
}