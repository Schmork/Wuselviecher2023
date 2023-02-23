using System.Collections.Generic;
using UnityEngine;

public class SensorController : MonoBehaviour
{
    readonly float circleDetectionRadius = 12f;
    readonly float circleSizeComparisonSafety = 0.9f; // reduce own size in comparisons as safety margin

    readonly float rectWidth = 1f;
    readonly float rectLength = 9f;

    // returns 12 resulting floats
    public float[] Scan()
    {
        var origin = (Vector2)transform.position;
        var halfExtents = new Vector2(rectWidth / 2.0f, rectLength / 2.0f) * transform.localScale.magnitude;
        var ahead = Physics2D.OverlapAreaAll(origin - halfExtents, origin + halfExtents);
        var around = Physics2D.OverlapCircleAll(transform.position, circleDetectionRadius * transform.localScale.magnitude);

        var results = ParseHits(ahead);
        results.AddRange(ParseHits(around));
        return results.ToArray();
    }

    // returns 6 resulting floats
    private List<float> ParseHits(Collider2D[] hits)
    {
        float minDistSqrBig = Mathf.Infinity;
        float distSqrSmall;
        Transform nextBiggerCell = null;
        Transform nextSmallerCell = null;

        var results = new List<float>();
        foreach (var hit in hits)
        {
            if (!hit.gameObject.CompareTag("Edible") || hit.gameObject == gameObject) continue;

            // find smaller
            if (hit.transform.localScale.x < transform.localScale.x * circleSizeComparisonSafety)
            {
                //float distSqr = (hit.transform.position - transform.position).sqrMagnitude;
                if (nextSmallerCell == null || hit.transform.localScale.x > nextSmallerCell.localScale.x)
                {
                    distSqrSmall = Vector2.Distance(transform.position, FuturePosition(hit.transform));
                    nextSmallerCell = hit.transform;
                }
            }

            // find bigger
            if (hit.transform.localScale.x > transform.localScale.x * circleSizeComparisonSafety)
            {
                float distSqr = Vector2.Distance(transform.position, FuturePosition(hit.transform));
                //float distSqr = (hit.transform.position - transform.position).sqrMagnitude;
                if (distSqr < minDistSqrBig)
                {
                    minDistSqrBig = distSqr;
                    nextBiggerCell = hit.transform;
                }
            }
        }
        results.AddRange(ParseCell(nextSmallerCell));
        results.AddRange(ParseCell(nextBiggerCell));
        return results;
    }

    Vector2 FuturePosition(Transform other)
    {
        float distance = Vector2.Distance(transform.position, other.transform.position);
        float timeToTarget = distance / (GetComponent<Rigidbody2D>().velocity.magnitude + 0.01f) * Time.deltaTime;
        return (Vector2)other.transform.position + GetComponent<Rigidbody2D>().velocity * timeToTarget;
    }

    float[] ParseCell(Transform other)
    {
        var results = new float[3];
        if (other == null) return results;
        var futurePos = FuturePosition(other);
        results[0] = other.transform.localScale.x / transform.localScale.x / 100f;
        results[1] = Vector2.Distance(transform.position, futurePos);
        results[2] = Vector2.SignedAngle(transform.up, futurePos) / 180f;
        /*
        results[1] = Vector2.Distance(transform.position, other.transform.position)
            / circleDetectionRadius * transform.localScale.magnitude;
        results[2] = Vector2.SignedAngle(transform.up, other.transform.position - transform.position) / 180f;
        */
        //Debug.Log(results[0] + " | " + results[1] + " | " + results[2]);
        return results;
    }
}
