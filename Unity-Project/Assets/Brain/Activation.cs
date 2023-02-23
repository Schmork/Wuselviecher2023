using System;

public enum ActivationFunction
{
    Input,
    Sigmoid,
    TanH
}

public static class Activation
{
    public static float Evaluate(ActivationFunction function, float input)
    {
        return function switch
        {
            ActivationFunction.Input => input,
            ActivationFunction.Sigmoid => 1.0f / (1.0f + (float)Math.Exp(-input)),
            ActivationFunction.TanH => (float)Math.Tanh(input),
            _ => throw new ArgumentException("Unknown activation function"),
        };
    }
}