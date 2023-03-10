using Unity.Mathematics;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    SizeController sc;
    SensorController sensors;
    Rigidbody2D rb;
    StatsCollector stats;
    float4[] inputs;

    public NeuralNetwork Brain;
    float4[] sensorData = new float4[0];
    float4 actions;
    float lastSensorUse;
    float lastBrainUse;

    static readonly float brainPrice = 0.0003f;             // based on rough Stopwatch measurements 
    static readonly float sensorPrice = brainPrice * 5;     // based on rough Stopwatch measurements 

    void Start()
    {
        inputs = new float4[NeuralNetwork.numInputs / 4];
        sc = GetComponent<SizeController>();
        sensors = GetComponent<SensorController>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<StatsCollector>();
    }

    void Update()
    {
        if (transform.position.magnitude > WorldConfig.Instance.FenceRadius * math.sqrt(10 + sc.Size)) gameObject.SetActive(false);
        stats.UpdateScore(Valhalla.Metric.FastestSpeedAchieved, rb.velocity.magnitude);
        stats.AddToScore(Valhalla.Metric.DistanceTravelled, rb.velocity.magnitude / sc.Size * Time.deltaTime);

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
        }

        for (i = 0; i < sensorData.Length; i++)
        {
            inputs[n++] = sensorData[i];
        }

        if (lastBrainUse > actions.z)
        {
            actions = Brain.FeedForward(inputs)[0];
            lastBrainUse = 0;
            sc.Size -= brainPrice;
        }

        lastSensorUse += Time.deltaTime;
        lastBrainUse += Time.deltaTime;

        if (actions.w < 0) return;

        var torque = 50f / Mathf.Pow(sc.Size + 1, 0.6f);
        var thrust = 50f * sc.Size;

        if (actions.w > 0.01) stats.ActionsTaken++;
        if (math.abs(actions.x) > 0.01) stats.ActionsTaken++;

        rb.AddForce(transform.up * actions.w * thrust * Time.deltaTime);
        rb.AddTorque(actions.x * torque * Time.deltaTime);
    }
}