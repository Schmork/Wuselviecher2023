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

    void Start()
    {
        inputs = new float[4 + SensorController.numSensorValues];
        sc = GetComponent<SizeController>();
        sensors = GetComponent<SensorController>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<StatsCollector>();
    }

    void Update()
    {
        if (transform.position.magnitude > 230 /*|| float.IsNaN(sc.Size)*/) gameObject.SetActive(false);
        if (rb.velocity.magnitude > stats.FastestSpeedAchieved) stats.FastestSpeedAchieved = rb.velocity.magnitude;
        stats.DistanceTravelled += rb.velocity.magnitude / sc.Size * Time.deltaTime;

        int n = 0;
        inputs[n++] = sc.Size / 100f;
        inputs[n++] = rb.velocity.magnitude / 10f;
        inputs[n++] = Vector2.Dot(rb.velocity, transform.up);
        inputs[n++] = System.MathF.Tanh(rb.angularVelocity / 900f);

        float[] data = sensors.Scan();
        for (int i = 0; i < data.Length; i++)
        {
            inputs[n++] = data[i];
        }

        var actions = Brain.FeedForward(inputs);

        var torque = 50f / Mathf.Pow(sc.Size + 1, 0.6f);// / (Mathf.Abs(rb.angularVelocity) + 0.0001f);
        var thrust = 50f * sc.Size;

        if (actions[0] > 0.01) stats.ActionsTaken++;
        if (actions[1] > 0.01) stats.ActionsTaken++;

        sc.Size -= Mathf.Abs(actions[0]) * sc.Size * Time.deltaTime / 20f / Mathf.Sqrt(rb.velocity.magnitude + 1);
        sc.Size -= Mathf.Abs(actions[1]) * sc.Size * Time.deltaTime / 5000f;

        if (actions[1] < 0) return;

        rb.AddTorque(actions[0] * torque * Time.deltaTime);
        rb.AddForce(transform.up * actions[1] * thrust * Time.deltaTime);
    }
}