using System.Collections.Generic;
using UnityEngine;

public class SensorController : MonoBehaviour
{
    readonly float circleDetectionRadius = 12f;
    readonly float circleSizeComparisonSafety = 0.9f; // reduce own size in comparisons as safety margin

    readonly float rectWidth = 1f;
    readonly float rectLength = 9f;

    readonly static int numTrackedCellsPerSensor = 7;
    readonly public static int numSensorValues = /*2 **/ numTrackedCellsPerSensor * 3;     // numSensors * numTracked * numValues

    // returns 12 resulting floats
    public float[] Scan()
    {
        //var origin = (Vector2)transform.position;
        //var halfExtents = new Vector2(rectWidth / 2.0f, rectLength / 2.0f) * transform.localScale.magnitude;
        //var ahead = Physics2D.OverlapAreaAll(origin - halfExtents, origin + halfExtents);
        var around = Physics2D.OverlapCircleAll(transform.position, circleDetectionRadius * transform.localScale.magnitude);

        //var results = ParseHits(ahead);
        //results.AddRange(ParseHits(around));
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

    // returns 6 resulting floats
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
        foreach (var hit in hits)
        {
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

    Vector2 FuturePosition(Vector3 otherPos)
    {
        float distance = Vector2.Distance(transform.position, otherPos);
        float timeToTarget = distance / (GetComponent<Rigidbody2D>().velocity.magnitude + 0.01f) * Time.deltaTime;
        return (Vector2)otherPos + GetComponent<Rigidbody2D>().velocity * timeToTarget;
    }

    float[] ParseCell(Trans other)
    {
        var results = new float[3];
        var futurePos = other.position;// FuturePosition(other.position);
        results[0] = other.localScale.x / transform.localScale.x / 100f;
        results[1] = Vector2.Distance(transform.position, futurePos);
        results[2] = Vector2.SignedAngle(transform.up, futurePos) / 180f;
        return results;
    }
}
