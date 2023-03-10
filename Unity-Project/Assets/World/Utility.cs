using Random = Unity.Mathematics.Random;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;
using System.Collections.Generic;

public class Utility : MonoBehaviour
{
    public static Random Random;

    static readonly Queue<float> gauss = new Queue<float>(1);

    void Awake()
    {
        Random = new Random((uint)System.DateTime.Now.Ticks);
    }

    [BurstCompile]
    // Generate two independent gaussian random numbers with mean 0 and variance 1
    public static float Gauss(float std = 1, float mean = 0)
    {
        if (!Application.isPlaying) return float.NaN;
        if (gauss.Count > 0) return gauss.Dequeue();

        // Generate two uniformly distributed random numbers in [0,1]
        var u1 = Random.NextFloat();
        var u2 = Random.NextFloat();

        // Apply the Box-Muller formula
        gauss.Enqueue((mean + std * math.sqrt(-2 * math.log(u1)) * math.cos(2 * math.PI * u2)));
        return mean + std * math.sqrt(-2 * math.log(u1)) * math.sin(2 * math.PI * u2);
    }
}
