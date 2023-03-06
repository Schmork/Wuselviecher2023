using System.Collections.Generic;
using UnityEngine;

public class SensorController : MonoBehaviour
{
    const float circleDetectionRadius = 12f;
    const float circleSizeComparisonSafety = 0.9f; // reduce own size in comparisons as safety margin

    static readonly int numTrackedCellsPerSensor = 7;
    public static readonly int numSensorValues = numTrackedCellsPerSensor * 3 * 2;     // numTracked * numValues (for both big & small)

    public float[] Scan()
    {
        var around = Physics2D.OverlapCircleAll(transform.position, circleDetectionRadius * transform.localScale.magnitude);
        var results = ParseHits(around);
        return results.ToArray();
    }

    struct Trans
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;

        public Trans(Transform trans)
        {
            position = trans.position;
            rotation = trans.rotation;
            localScale = trans.localScale;
        }
    }

    private List<float> ParseHits(Collider2D[] hits)
    {
        var nullTrans = new Trans
        {
            position = Vector3.zero,
            rotation = Quaternion.identity,
            localScale = Vector3.zero
        };

        var biggerQueue = new SortedList<float, Trans>();
        var smallerQueue = new SortedList<float, Trans>();

        for (int i = 0; i < numTrackedCellsPerSensor; i++)
        {
            biggerQueue.Add(i / 1000f, nullTrans);
            smallerQueue.Add(i / 1000f, nullTrans);
        }

        var results = new List<float>();
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (!hit.gameObject.CompareTag("Edible") || hit.gameObject == gameObject) continue;

            var myScale = transform.localScale.x;
            var hitScale = hit.transform.localScale.x;

            // find smaller
            if (hitScale < myScale * circleSizeComparisonSafety)
            {
                if (hitScale > smallerQueue.Values[0].localScale.x)
                {
                    while (smallerQueue.ContainsKey(hitScale)) hitScale += 0.000001f;
                    smallerQueue.RemoveAt(0);
                    smallerQueue.Add(hitScale, new Trans(hit.transform));
                }
            }

            // find bigger
            if (hitScale > myScale * circleSizeComparisonSafety)
            {
                if (hitScale > biggerQueue.Values[0].localScale.x)
                {
                    while (biggerQueue.ContainsKey(hitScale)) hitScale += 0.000001f;
                    biggerQueue.RemoveAt(0);
                    biggerQueue.Add(hitScale, new Trans(hit.transform));
                }
            }
        }

        foreach (var item in smallerQueue)
        {
            results.AddRange(ParseCell(item.Value));
        }
        foreach (var item in biggerQueue)
        {
            results.AddRange(ParseCell(item.Value));
        }
        return results;
    }

    float[] ParseCell(Trans other)
    {
        var results = new float[3];
        var futurePos = other.position;
        results[0] = other.localScale.x / transform.localScale.x / 100f;
        results[1] = Vector2.Distance(transform.position, futurePos);
        results[2] = Vector2.SignedAngle(transform.up, futurePos) / 180f;
        return results;
    }
}
