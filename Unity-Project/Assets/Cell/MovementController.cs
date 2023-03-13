using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public class MovementController : MonoBehaviour
{
    [SerializeField] SizeController sc;
    [SerializeField] SensorController sensors;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] StatsCollector stats;

    public NeuralNetwork[] Brains = new NeuralNetwork[4];
    public NeuralNetwork ActiveBrain;
    
    float4[] sensorData = new float4[0];
    float4[] actions;
    float4[] inputs;
    
    float lastSensorUse;
    float lastBrainUse;

    static readonly float brainPrice = 0.0001f;             // based on rough Stopwatch measurements 
    static readonly float sensorPrice = brainPrice * 6.4f;  // based on rough Stopwatch measurements 

    void OnEnable()
    {
        inputs = new float4[NeuralNetwork.numInputs / 4];
        actions = new float4[2];
        ActiveBrain = Brains[0];
    }

    [BurstCompile]
    void Update()
    {
        stats.AddToScore(Valhalla.Metric.AverageSpeed, rb.velocity.magnitude * Time.deltaTime / 100f);
        stats.AddToScore(Valhalla.Metric.DistanceTravelled, rb.velocity.magnitude / sc.Size * Time.deltaTime);

        lastSensorUse += Time.deltaTime;
        lastBrainUse += Time.deltaTime;

        int i;
        int n = 0;
        for (i = 0; i < ActiveBrain.Memory.Length; i++)
        {
            inputs[n] = ActiveBrain.Layers[ActiveBrain.Memory[i].x].Memory[ActiveBrain.Memory[i].y];
            if (i / 4 > 0 && i % 4 == 0) n++;
        }

        inputs[n++] = new float4(
            sc.Size / 100f,
            rb.velocity.magnitude / 10f,
            Vector2.Dot(rb.velocity, transform.up),
            System.MathF.Tanh(rb.angularVelocity / 900f)
            );

        if (lastSensorUse > actions[0].y)
        {
            sensorData = sensors.Scan();
            lastSensorUse = 0;
            sc.Size -= sensorPrice;
            stats.ActionsTaken++;
        }

        for (i = 0; i < sensorData.Length; i++)
        {
            inputs[n++] = sensorData[i];
        }

        if (lastBrainUse > actions[0].z)
        {
            actions = ActiveBrain.FeedForward(inputs);
            actions[0].w = math.clamp(actions[0].w, 0, 1);
            actions[0].x = math.clamp(actions[0].x, -1, 1);

            lastBrainUse = 0;
            sc.Size -= brainPrice;
            stats.ActionsTaken++;

            ActiveBrain = Brains[actions[1] switch
            {
                _ when actions[1].w >= actions[1].x && actions[1].w >= actions[1].y && actions[1].w >= actions[1].z => 0,
                _ when actions[1].x >= actions[1].y && actions[1].x >= actions[1].z => 1,
                _ when actions[1].y >= actions[1].z => 2,
                _ => 3
            }];
        }

        var thrust = actions[0].w * 40f * math.sqrt(sc.Size);
        var torque = actions[0].x * 40f / Mathf.Pow(sc.Size + 1, 0.6f);

        rb.AddForce(thrust * Time.deltaTime * transform.up);
        rb.AddTorque(torque * Time.deltaTime);
    }
}