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
    float4[] sensorData;
    float4[] actionVector = new float4[2];
    float lastSensorUse;
    float lastBrainUse;

    static readonly float brainPrice = 0.001f;              // based on rough Stopwatch measurements 
    static readonly float sensorPrice = brainPrice * 15;    // based on rough Stopwatch measurements 

    void Start()
    {
        inputs = new float4[(4 + SensorController.numSensorValues) / 4];
        sc = GetComponent<SizeController>();
        sensors = GetComponent<SensorController>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<StatsCollector>();
    }

    void Update()
    {
        if (transform.position.magnitude > WorldConfig.Instance.FenceRadius * math.sqrt(10 + sc.Size)) gameObject.SetActive(false);
        if (rb.velocity.magnitude > stats.FastestSpeedAchieved) stats.FastestSpeedAchieved = rb.velocity.magnitude;
        stats.DistanceTravelled += rb.velocity.magnitude / sc.Size * Time.deltaTime;

        int n = 0;
        inputs[n++] = new float4(
            sc.Size / 100f,
            rb.velocity.magnitude / 10f,
            Vector2.Dot(rb.velocity, transform.up),
            System.MathF.Tanh(rb.angularVelocity / 900f)
            );

        if (Time.time - lastSensorUse > actionVector[0].y)
        {
            sensorData = sensors.Scan();
            lastSensorUse = Time.time;
            sc.Size -= sensorPrice;
        }
        for (int i = 0; i < sensorData.Length; i++)
        {
            inputs[n++] = sensorData[i];
        }

        if (Time.time - lastBrainUse > actionVector[0].z)
        {
            actionVector = Brain.FeedForward(inputs);
            lastBrainUse = Time.time;
            sc.Size -= brainPrice;
        }
        var action = actionVector[0];

        var torque = 50f / Mathf.Pow(sc.Size + 1, 0.6f);// / (Mathf.Abs(rb.angularVelocity) + 0.0001f);
        var thrust = 50f * sc.Size;

        if (action.w > 0.01) stats.ActionsTaken++;
        if (action.x > 0.01) stats.ActionsTaken++;

        sc.Size -= Mathf.Abs(action.w) * sc.Size * Time.deltaTime / 20f / Mathf.Sqrt(rb.velocity.magnitude + 1);
        sc.Size -= Mathf.Abs(action.x) * sc.Size * Time.deltaTime / 5000f;

        if (action.x < 0) return;

        rb.AddTorque(action.w * torque * Time.deltaTime);
        rb.AddForce(transform.up * action.x * thrust * Time.deltaTime);
    }
}