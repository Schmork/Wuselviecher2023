using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;
using System;
using Unity.Mathematics;
using System.Linq;
using Unity.Collections;

[BurstCompile]
[Serializable]
public class Layer : ICloneable
{
    public ActivationFunction[] NeuronFunctions;
    public float4[] NeuronBias;
    public float4[] Weights;
    public float4[] Cache;

    public Layer(int neuronCount, int inputLength, ActivationFunction? function = null)
    {
        NeuronFunctions = new ActivationFunction[neuronCount];
        NeuronBias = new float4[neuronCount / 4];
        Cache = new float4[neuronCount / 4];
        Weights = new float4[neuronCount * inputLength / 4];

        int i;
        for (i = 0; i < NeuronBias.Length; i++)
        {
            NeuronBias[i] = RandomInitialValue();
            NeuronFunctions[i] = function == null ? RandomFunction() : (ActivationFunction)function;
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

    public Layer(float4[] weights, float4[] neuronBias, float4[] cache, ActivationFunction[] neuronFunction)
    {
        NeuronFunctions = neuronFunction;
        NeuronBias = neuronBias;
        Weights = weights;
        Cache = cache;
    }

    public static ActivationFunction RandomFunction()
    {
        var functions = Enum.GetValues(typeof(ActivationFunction)) as ActivationFunction[];
        var randomIndex = 1 + Utility.Random.NextInt(functions.Length - 1); // skip "INPUT"
        return functions[randomIndex];
    }

    static float4 RandomInitialValue()
    {
        var initial = WorldConfig.Instance.InitialValues;
        return new float4(
            Utility.Gauss(initial),
            Utility.Gauss(initial),
            Utility.Gauss(initial),
            Utility.Gauss(initial)
            );
    }

    public object Clone()
    {return new Layer(
            Weights.Clone() as float4[],
            NeuronBias.Clone() as float4[],
            Cache.Clone() as float4[],
            NeuronFunctions.Clone() as ActivationFunction[]
            );
    }

    /*
    public struct FeedForwardJob : IJobParallelFor
    {
        public readonly NativeArray<int4> NeuronFunctions;
        public readonly NativeArray<float4> NeuronBias;
        public readonly NativeArray<float4> Weights;
        public readonly NativeArray<float4> input;
        public NativeArray<float4> output;

        public FeedForwardJob(int4[] nf, float4[] nb, float4[] wg, float4[] it, float4[] ot)
        {
            NeuronFunctions = new NativeArray<int4>(nf, Allocator.TempJob);
            NeuronBias = new NativeArray<float4>(nb, Allocator.TempJob);
            Weights = new NativeArray<float4>(wg, Allocator.TempJob);
            input = new NativeArray<float4>(it, Allocator.TempJob);
            output = new NativeArray<float4>(ot, Allocator.TempJob);
        }

        [BurstCompile]
        public void Execute(int i)
        {

            float4x4 inputMatrix = new float4x4(input[0], input[1], input[2], input[3]);
            int w = i * 4; // use w instead of i for indexing Weights
            float4x4 weightMatrix = new float4x4(
                Weights[w + 0].w, Weights[w + 0].x, Weights[w + 0].y, Weights[w + 0].z,
                Weights[w + 1].w, Weights[w + 1].x, Weights[w + 1].y, Weights[w + 1].z,
                Weights[w + 2].w, Weights[w + 2].x, Weights[w + 2].y, Weights[w + 2].z,
                Weights[w + 3].w, Weights[w + 3].x, Weights[w + 3].y, Weights[w + 3].z
            );
            var sum4 = math.mul(inputMatrix, weightMatrix);

            float sum = 0f;
            for (int j = 0; j < input.Length; j++)
            {
                sum += math.dot(Weights[i * input.Length + j], input[j]);
            }
            output[i] = new float4(
                Activation.Evaluate(NeuronFunctions[i].w, NeuronBias[i].w * sum),
                Activation.Evaluate(NeuronFunctions[i].x, NeuronBias[i].x * sum),
                Activation.Evaluate(NeuronFunctions[i].y, NeuronBias[i].y * sum),
                Activation.Evaluate(NeuronFunctions[i].z, NeuronBias[i].z * sum)
                );
        }
    }

    [BurstCompile]
    public float4[] FeedForward(float4[] input)
    {
        float4[] output = new float4[NeuronBias.Length];
        FeedForwardJob job = new FeedForwardJob(
            FunctionIndices,
            NeuronBias,
            Weights,
            input,
            output
        );
        job.Schedule(NeuronBias.Length, 2048, default).Complete();
        return output;
    }
    */

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
            Cache[i] = output[i];
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