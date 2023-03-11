using Unity.Burst;
using Unity.Mathematics;

public enum ActivationFunction
{
    Identity,
    Sigmoid,
    TanH,
    Gaussian,
    Sine, 
    Cos,
    LeakyReLU,
    ArcTan
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
            ActivationFunction.Cos => math.cos(x),
            ActivationFunction.LeakyReLU => math.max(0.01f * x, x),
            ActivationFunction.ArcTan => math.atan(x),
            _ => throw new System.ArgumentException("Unknown activation function"),
        };
    }

    [BurstCompile]
    public static float Evaluate(int i, float x)
    {
        return (ActivationFunction)i switch
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