using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    SizeController sc;
    SensorController sensors;
    Rigidbody2D rb;
    StatsCollector stats;
    float[] inputs;

    public NeuralNetwork Brain
    {
        get;
        private set;
    }

    public void SetBrain(NeuralNetwork brain)
    {
        Brain = brain;
    }

    private void Start()
    {
        inputs = new float[3 + Brain.memNeuronIndex.Length + SensorController.numSensorValues];
        sc = GetComponent<SizeController>();
        sensors = GetComponent<SensorController>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<StatsCollector>();
        stats.TimeSpawned = Time.time;
    }

    void Update()
    {
        if (rb.velocity.magnitude > stats.FastestSpeedAchieved) stats.FastestSpeedAchieved = rb.velocity.magnitude;
        stats.DistanceTravelled += rb.velocity.magnitude / sc.Size * Time.deltaTime;

        int n = 0;
        inputs[n++] = sc.Size / 100f;
        inputs[n++] = rb.velocity.magnitude / 10f;
        inputs[n++] = System.MathF.Tanh(rb.angularVelocity / 900f);

        for (int i = 0; i < Brain.memNeuronIndex.Length; i++)
        {
            inputs[n++] = Brain.Layers[Brain.memNeuronLayer[i]].PrevOutput[Brain.memNeuronIndex[i]];
        }

        float[] data = sensors.Scan();
        for (int i = 0; i < data.Length; i++)
        {
            inputs[n++] = data[i];
        }

        var actions = Brain.FeedForward(inputs);

        var torque = 1500f / Mathf.Pow(sc.Size + 1, 0.6f);// / (Mathf.Abs(rb.angularVelocity) + 0.0001f);
        var thrust = 250f * sc.Size;

        if (actions[0] > 0.01) stats.ActionsTaken++;
        if (actions[1] > 0.01) stats.ActionsTaken++;

        sc.Size -= Mathf.Abs(actions[0]) * sc.Size * Time.deltaTime / 8f / Mathf.Sqrt(rb.velocity.magnitude + 1);
        sc.Size -= Mathf.Abs(actions[1]) * sc.Size * Time.deltaTime / 1500f;

        if (actions[1] < 0) return;

        rb.AddTorque(actions[0] * torque * Time.deltaTime);
        rb.AddForce(transform.up * actions[1] * thrust * Time.deltaTime);
    }
}