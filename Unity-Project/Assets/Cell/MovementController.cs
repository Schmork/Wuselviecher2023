using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    SizeController sc;
    SensorController sensors;
    Rigidbody2D rb;
    StatsCollector stats;
    System.Diagnostics.Stopwatch stopwatch;

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
        sc = GetComponent<SizeController>();
        sensors = GetComponent<SensorController>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<StatsCollector>();
        stats.TimeSpawned = Time.time;
        stopwatch = new System.Diagnostics.Stopwatch();
    }

    void Update()
    {
        if (rb.velocity.magnitude > stats.FastestSpeedAchieved) stats.FastestSpeedAchieved = rb.velocity.magnitude;
        stats.DistanceTravelled += rb.velocity.magnitude / sc.Size * Time.deltaTime;


        var inputList = new List<float> { sc.Size / 500f, rb.velocity.magnitude / 10f, rb.angularVelocity / 50f };

        for (int i = 0; i < Brain.memNeuronIndex.Length; i++)
        {
            var layer = Brain.memNeuronLayer[i];
            var neuron = Brain.memNeuronIndex[i];
            inputList.Add(Brain.Layers[layer].PrevOutput[neuron]);
        }

        inputList.AddRange(sensors.Scan());
        
        stopwatch.Start();
        var actions = Brain.FeedForward(inputList.ToArray());
        WorldController.spans.Add(stopwatch.ElapsedTicks);
        stopwatch.Reset();

        var torque = 500f / Mathf.Pow(sc.Size + 1, 0.7f) / (Mathf.Abs(rb.angularVelocity) + 0.0001f);
        var thrust = 250f * sc.Size;
        
        rb.AddTorque(actions[0] * torque * Time.deltaTime);
        rb.AddForce(transform.up * actions[1] * thrust * Time.deltaTime);

        if (actions[0] > 0.01) stats.ActionsTaken++;
        if (actions[1] > 0.01) stats.ActionsTaken++;

        sc.Size -= Mathf.Abs(actions[0]) * sc.Size * Time.deltaTime / 500f;
        sc.Size -= actions[1] * sc.Size * Time.deltaTime / 500f;
    }
}