using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using System.Collections.Generic;

[BurstCompile]
public class SensorController : MonoBehaviour
{
    const float circleDetectionRadius = 12f;
    const float circleSizeComparisonSafety = 0.9f; // reduce own size in comparisons as safety margin

    static readonly int numTrackedCellsPerSensor = 10;
    public static readonly int numSensorValues = numTrackedCellsPerSensor * 3 * 2;     // numTracked * numValues (for both big & small)
    // make sure numSensorValues % 4 == 0 so we can use float4 operations

    public float4[] Scan()
    {
        var around = Physics2D.OverlapCircleAll(transform.position, circleDetectionRadius * transform.localScale.magnitude);
        return ParseHits(around);
    }

    struct Trans
    {
        public float2 position;
        public float localScale;
        public float2 velocity;

        public Trans(Transform trans, Vector3 velocity)
        {
            position = new float2(trans.position.x, trans.position.y);
            localScale = trans.localScale.x;
            this.velocity = new float2(velocity.x, velocity.y);
        }
    }

    [BurstCompile]
    private float4[] ParseHits(Collider2D[] hits)
    {
        int i;
        var nullTrans = new Trans
        {
            position = float2.zero,
            localScale = 0
        };

        var biggerQueue = new SortedList<float, Trans>();
        var smallerQueue = new SortedList<float, Trans>();
        
        for (i = 0; i < numTrackedCellsPerSensor; i++)
        {
            biggerQueue.Add(i / 1000f, nullTrans);
            smallerQueue.Add(i / 1000f, nullTrans);
        }

        var results = new List<float>();
        for (i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (!hit.gameObject.CompareTag("Edible") || hit.gameObject == gameObject) continue;

            var myScale = transform.localScale.x;
            var hitScale = hit.transform.localScale.x;

            // find smaller
            if (hitScale < myScale * circleSizeComparisonSafety)
            {
                if (hitScale > smallerQueue.Values[0].localScale)
                {
                    while (smallerQueue.ContainsKey(hitScale)) hitScale += 0.000001f;
                    smallerQueue.RemoveAt(0);
                    smallerQueue.Add(hitScale, new Trans(hit.transform, hit.attachedRigidbody.velocity));
                }
            }

            // find bigger
            if (hitScale > myScale * circleSizeComparisonSafety)
            {
                if (hitScale > biggerQueue.Values[0].localScale)
                {
                    while (biggerQueue.ContainsKey(hitScale)) hitScale += 0.000001f;
                    biggerQueue.RemoveAt(0);
                    biggerQueue.Add(hitScale, new Trans(hit.transform, hit.attachedRigidbody.velocity));
                }
            }
        }

        foreach (var trans in smallerQueue)
        {
            results.AddRange(ParseCell(trans.Value));
        }
        foreach (var trans in biggerQueue)
        {
            results.AddRange(ParseCell(trans.Value));
        }

        Debug.Assert(results.Count % 4 == 0);
        float4[] results4 = new float4[results.Count / 4];
        for (i = 0; i < results.Count; i += 4)
        {
            results4[i / 4] = new float4(results[i], results[i + 1], results[i + 2], results[i + 3]);
        }
        return results4;
    }

    [BurstCompile]
    float[] ParseCell(Trans other)
    {
        var results = new float[3];
        var futurePos = other.position + other.velocity * Time.deltaTime;
        results[0] = other.localScale / transform.localScale.x / 10f;
        results[1] = math.distancesq(new float2(transform.position.x, transform.position.y), futurePos);
        results[2] 
            = (math.atan2(futurePos.y - transform.position.y, futurePos.x - transform.position.x) 
            - math.atan2(transform.up.y, transform.up.x)) / math.PI;
        return results;
    }
}
