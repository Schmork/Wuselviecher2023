using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using System.Collections.Generic;

[BurstCompile]
public class SensorController : MonoBehaviour
{
    [SerializeField] StatsCollector stats;

    const float circleDetectionRadius = 3.2f;
    const float circleSizeComparisonSafety = 0.9f; // reduce own size in comparisons as safety margin
    
    static readonly int numTrackedCellsPerSensor = 2;
    public static readonly int numSensorValues = numTrackedCellsPerSensor * 3 * 2;     // numTracked * numValues (for both big & small)
                                                                                       // make sure numSensorValues % 4 == 0 so we can use float4 operations
    [BurstCompile]
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
        var myPos = V3_to_float2(transform.position);

        int i;
        var nullTrans = new Trans
        {
            position = float2.zero,
            localScale = 0
        };

        var biggerQueue = new SortedList<float, Trans>(numTrackedCellsPerSensor);
        var smallerQueue = new SortedList<float, Trans>(numTrackedCellsPerSensor);

        var myScale = transform.localScale.x;
        for (i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (hit.gameObject == gameObject) continue;

            var hitScale = hit.transform.localScale.x;
            var sqDist = math.distancesq(myPos, V3_to_float2(hit.transform.position));

            // find smaller
            if (hitScale < myScale * circleSizeComparisonSafety)
            {
                while (smallerQueue.ContainsKey(hitScale)) hitScale += 0.001f;
                if (smallerQueue.Count < numTrackedCellsPerSensor)
                {
                    smallerQueue.Add(sqDist / hitScale, new Trans(hit.transform, hit.attachedRigidbody.velocity));
                }
                else if (sqDist / hitScale < smallerQueue.Keys[0])
                {
                    smallerQueue.RemoveAt(0);
                    smallerQueue.Add(sqDist / hitScale, new Trans(hit.transform, hit.attachedRigidbody.velocity));
                }
            }

            // find bigger
            else
            {
                while (biggerQueue.ContainsKey(sqDist)) sqDist += 0.001f;
                if (biggerQueue.Count < numTrackedCellsPerSensor)
                {
                    biggerQueue.Add(sqDist, new Trans(hit.transform, hit.attachedRigidbody.velocity));
                }
                else if (sqDist < biggerQueue.Keys[0])
                {
                    biggerQueue.RemoveAt(0);
                    biggerQueue.Add(sqDist, new Trans(hit.transform, hit.attachedRigidbody.velocity));
                }
            }
        }

        var results = new float[numSensorValues];
        int j;
        for (i = 0; i < numSensorValues / 2; i += 3)
        {
            var trans = smallerQueue.Count <= i ? nullTrans : smallerQueue.Values[i / 3];
            for (j = 0; j < 3; j++)
            {
                results[i + j] = ParseCell(trans)[j];
            }
        }

        for (i = 0; i < numSensorValues / 2; i += 3)
        {
            var trans = biggerQueue.Count <= i ? nullTrans : biggerQueue.Values[i / 3];
            for (j = 0; j < 3; j++)
            {
                var bigger = ParseCell(trans);
                results[i + j + numSensorValues / 2] = bigger[j];
                if (biggerQueue.Count == 0 && myScale < WorldController.BiggestCell)
                    stats.AddToScore(
                        Valhalla.Metric.PeaceTime,
                        myScale / 100f * Time.deltaTime
                        );
            }
        }

        float4[] results4 = new float4[results.Length / 4];
        for (i = 0; i < results.Length; i += 4)
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
        results[1] = math.distancesq(V3_to_float2(transform.position), futurePos);
        results[2]
            = (math.atan2(futurePos.y - transform.position.y, futurePos.x - transform.position.x)
            - math.atan2(transform.up.y, transform.up.x)) / math.PI;
        return results;
    }

    float2 V3_to_float2(Vector3 pos)
    {
        return new float2(pos.x, pos.y);
    }
}