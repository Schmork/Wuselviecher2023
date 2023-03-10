using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public class MovementController : MonoBehaviour
{
    [SerializeField] SizeController sc;
    [SerializeField] SensorController sensors;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] StatsCollector stats;
    float4[] inputs;

    public NeuralNetwork Brain;
    float4[] sensorData = new float4[0];
    float4 actions;
    float lastSensorUse;
    float lastBrainUse;

    static readonly float brainPrice = 0.0003f;             // based on rough Stopwatch measurements 
    static readonly float sensorPrice = brainPrice * 5;     // based on rough Stopwatch measurements 

    void OnEnable()
    {
        inputs = new float4[NeuralNetwork.numInputs / 4];
    }

    [BurstCompile]
    void Update()
    {
        if (transform.position.magnitude > WorldConfig.Instance.FenceRadius * math.sqrt(10 + sc.Size)) gameObject.SetActive(false);
        stats.AddToScore(Valhalla.Metric.AverageSpeed, rb.velocity.magnitude * Time.deltaTime / 100f);
        stats.AddToScore(Valhalla.Metric.DistanceTravelled, rb.velocity.magnitude / sc.Size * Time.deltaTime);

        lastSensorUse += Time.deltaTime;
        lastBrainUse += Time.deltaTime;

        int i;
        int n = 0;
        for (i = 0; i < Brain.Memory.Length; i++)
        {
            inputs[n] = Brain.Layers[Brain.Memory[i].x].Cache[Brain.Memory[i].y];
            if (i / 4 > 0 && i % 4 == 0) n++;
        }

        inputs[n++] = new float4(
            sc.Size / 100f,
            rb.velocity.magnitude / 10f,
            Vector2.Dot(rb.velocity, transform.up),
            System.MathF.Tanh(rb.angularVelocity / 900f)
            );

        if (lastSensorUse > actions.y)
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

        if (lastBrainUse > actions.z)
        {
            actions = Brain.FeedForward(inputs)[0];
            actions.w = math.clamp(actions.w, 0, 1);
            actions.x = math.clamp(actions.x, -1, 1);
            lastBrainUse = 0;
            sc.Size -= brainPrice;
            stats.ActionsTaken++;
        }

        var thrust = actions.w * 50f * sc.Size;
        var torque = actions.x * 50f / Mathf.Pow(sc.Size + 1, 0.6f);

        rb.AddForce(thrust * Time.deltaTime * transform.up);
        rb.AddTorque(torque * Time.deltaTime);
    }
}