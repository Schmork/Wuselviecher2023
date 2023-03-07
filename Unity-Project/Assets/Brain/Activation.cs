using System;
using UnityEngine;

public enum ActivationFunction
{
    Input,
    Sigmoid,
    TanH,
    Gaussian,
    Sine,
    Identity
}

public static class Activation
{
    public static float Evaluate(ActivationFunction function, float x)
    {
        return function switch
        {
            ActivationFunction.Input => x,
            ActivationFunction.Sigmoid => 1.0f / (1.0f + (float)Math.Exp(-x)),
            ActivationFunction.TanH => (float)Math.Tanh(x),
            ActivationFunction.Gaussian => Mathf.Exp(-(x * x)),
            ActivationFunction.Sine => MathF.Sin(x),
            ActivationFunction.Identity => x,
            _ => throw new ArgumentException("Unknown activation function"),
        };
    }
}