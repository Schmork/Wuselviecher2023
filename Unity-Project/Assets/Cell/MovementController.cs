using SharpNeat.Phenomes;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    SizeController sc;
    Rigidbody2D rb;
    StatsCollector stats;

    private void Start()
    {
        sc = GetComponent<SizeController>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<StatsCollector>();
        stats.TimeSpawned = Time.time;
    }

    void Update()
    {
        if (rb.velocity.magnitude > stats.FastestSpeedAchieved) stats.FastestSpeedAchieved = rb.velocity.magnitude;
        stats.DistanceTravelled += rb.velocity.magnitude / sc.Size * Time.deltaTime;
    }

    public void UseBlackBoxOutpts(ISignalArray outputSignalArray)
    {
        if (sc == null || rb == null) return;

        var actions = new float[2];
        actions[0] = (float)outputSignalArray[0];
        actions[1] = (float)outputSignalArray[1];

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