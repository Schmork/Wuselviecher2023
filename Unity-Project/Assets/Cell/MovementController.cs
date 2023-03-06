using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    SizeController sc;
    SensorController sensors;
    Rigidbody2D rb;
    StatsCollector stats;

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
    }

    static float maxSize = 0;
    static float maxSpeed = 0;
    static float maxAngular = 0;

    void Update()
    {
        if (rb.velocity.magnitude > stats.FastestSpeedAchieved) stats.FastestSpeedAchieved = rb.velocity.magnitude;
        stats.DistanceTravelled += rb.velocity.magnitude / sc.Size * Time.deltaTime;

        var inputList = new List<float> { sc.Size / 100f, rb.velocity.magnitude / 10f, System.MathF.Tanh(rb.angularVelocity / 900f) };

        var hasChanged = false;
        if (sc.Size > maxSize)
        {
            maxSize = sc.Size;
            hasChanged = true;
        }
        if (rb.velocity.magnitude > maxSpeed)
        {
            maxSpeed = rb.velocity.magnitude;
            hasChanged = true;
        }
        if (Mathf.Abs(rb.angularVelocity) > maxAngular)
        {
            maxAngular = Mathf.Abs(rb.angularVelocity);
            hasChanged = true;
        }
        //if (hasChanged) Debug.Log("Max Size: " + maxSize + "  Max Speed: " + maxSpeed + "  Max Angular: " + maxAngular);


        for (int i = 0; i < Brain.memNeuronIndex.Length; i++)
        {
            var layer = Brain.memNeuronLayer[i];
            var neuron = Brain.memNeuronIndex[i];
            inputList.Add(Brain.Layers[layer].PrevOutput[neuron]);
        }

        inputList.AddRange(sensors.Scan());

        var actions = Brain.FeedForward(inputList.ToArray());

        if (float.IsNaN(actions[0])) Debug.Log("0");
        if (float.IsNaN(actions[1])) Debug.Log("1");

        var torque = 1500f / Mathf.Pow(sc.Size + 1, 0.6f);// / (Mathf.Abs(rb.angularVelocity) + 0.0001f);
        var thrust = 250f * sc.Size;

        if (actions[0] > 0.01) stats.ActionsTaken++;
        if (actions[1] > 0.01) stats.ActionsTaken++;

        sc.Size -= Mathf.Abs(actions[0]) * sc.Size * Time.deltaTime / 8f / Mathf.Sqrt(rb.velocity.magnitude + 1);
        sc.Size -= Mathf.Abs(actions[1]) * sc.Size * Time.deltaTime / 800f;

        if (actions[1] < 0) return;

        rb.AddTorque(actions[0] * torque * Time.deltaTime);
        rb.AddForce(transform.up * actions[1] * thrust * Time.deltaTime);
    }
}