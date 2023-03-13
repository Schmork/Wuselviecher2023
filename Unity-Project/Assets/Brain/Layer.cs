using System;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
[Serializable]
public class Layer : ICloneable
{
    public ActivationFunction[] Functions;
    public float4[] Biases;
    public float4[] Weights;
    public float4[] Memory;

    public Layer(int neuronCount, int inputLength, bool isInputLayer = false)
    {
        Functions = new ActivationFunction[isInputLayer ? 0 : neuronCount];
        Biases = new float4[neuronCount / 4];
        Memory = new float4[neuronCount / 4];
        Weights = new float4[neuronCount * inputLength / 4];

        int i;
        for (i = 0; i < Biases.Length; i++)
        {
            Biases[i] = RandomInitialValue();
            if (!isInputLayer) Functions[i] = RandomFunction();
        }
        for (i = 0; i < Weights.Length; i++)
            Weights[i] = RandomInitialValue();
    }

    public static ActivationFunction RandomFunction()
    {
        var functions = Enum.GetValues(typeof(ActivationFunction)) as ActivationFunction[];
        var randomIndex = Utility.Random.NextInt(functions.Length);
        return functions[randomIndex];
    }

    static float4 RandomInitialValue()
    {
        return new float4(
            Utility.Gauss(WorldConfig.GaussStd),
            Utility.Gauss(WorldConfig.GaussStd),
            Utility.Gauss(WorldConfig.GaussStd),
            Utility.Gauss(WorldConfig.GaussStd)
            );
    }

    [BurstCompile]
    public float4[] FeedForwardInput(float4[] input)
    {
        float4[] output = new float4[Biases.Length];
        for (int i = 0; i < Biases.Length; i++)
        {
            output[i] = Biases[i] + input[i];
            Memory[i] = output[i];
        }
        return output;
    }

    [BurstCompile]
    public float4[] FeedForward(float4[] input)
    {
        float sum;
        float4[] output = new float4[Biases.Length];
        for (int i = 0; i < Biases.Length; i++)
        {
            var weightSum = Biases[i];
            for (int j = 0; j < input.Length; j++)
                weightSum += Weights[i * input.Length + j] * input[j];
            sum = math.csum(weightSum);
            output[i].w = Activation.Evaluate(Functions[i + 0], sum);
            output[i].x = Activation.Evaluate(Functions[i + 1], sum);
            output[i].y = Activation.Evaluate(Functions[i + 2], sum);
            output[i].z = Activation.Evaluate(Functions[i + 3], sum);
            Memory[i] = output[i];
        }
        return output;
    }

    Layer(float4[] memory, float4[] weights, float4[] biases, ActivationFunction[] functions)
    {
        Memory = memory;
        Weights = weights;
        Biases = biases;
        Functions = functions;
    }

    public object Clone()
    {
        return new Layer(
               Memory.Clone() as float4[],
               Weights.Clone() as float4[],
               Biases.Clone() as float4[],
               Functions.Clone() as ActivationFunction[]
               );
    }
}