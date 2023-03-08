using Unity.Burst;
using Unity.Mathematics;

public enum ActivationFunction
{
    Identity,
    Sigmoid,
    TanH,
    Gaussian,
    Sine
}

[BurstCompile]
public static class Activation
{
    [BurstCompile]
    public static float Evaluate(ActivationFunction function, float x)
    {
        return function switch
        {
            ActivationFunction.Identity => x,
            ActivationFunction.Sigmoid => 1.0f / (1.0f + math.exp(-x)),
            ActivationFunction.TanH => math.tanh(x),
            ActivationFunction.Gaussian => math.exp(-(math.lengthsq(x))),
            ActivationFunction.Sine => math.sin(x),
            _ => throw new System.ArgumentException("Unknown activation function"),
        };
    }
}