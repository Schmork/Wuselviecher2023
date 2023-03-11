using UnityEngine;

public class CollisionController : MonoBehaviour
{
    [SerializeField] SizeController sc;
    [SerializeField] Rigidbody2D Rb;
    [SerializeField] SpriteRenderer rendr;
    [SerializeField] StatsCollector stats;

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (!collider.CompareTag("Edible")) return;

        var other = collider.GetComponent<SizeController>();
        if (other.Size > sc.Size) return;
        var diff = sc.Size / 30f;
        if (other.Size < diff) diff = other.Size;

        var col1 = rendr.material.color;
        var col2 = other.GetComponent<SpriteRenderer>().material.color;
        var col = MixColors(col1, col2, diff / sc.Size);
        rendr.material.color = col;

        sc.Size += diff;
        other.Size -= diff;

        stats.AddToScore(Valhalla.Metric.MassEatenAtSpeed, diff * (0.01f + Mathf.Sqrt(Rb.velocity.magnitude * Time.deltaTime)));
        stats.AddToScore(Valhalla.Metric.MassEaten, diff);
        stats.AddToScore(Valhalla.Metric.StraightMass, diff / Mathf.Exp(Mathf.Abs(Rb.angularVelocity * Time.deltaTime)));

        var otherCc = collider.GetComponent<CollisionController>();
        if (otherCc == null) return;
        Rb.AddForce(otherCc.Rb.velocity * other.Size * Time.deltaTime);
        otherCc.Rb.AddForce(Rb.velocity * sc.Size * Time.deltaTime);
    }

    private Color MixColors(Color color1, Color color2, float ratio)
    {
        var ampFactor = 0.1f;

        // Get RGB values of the two colors
        float r = color1.r * (1 - ratio) + color2.r * ratio;
        float g = color1.g * (1 - ratio) + color2.g * ratio;
        float b = color1.b * (1 - ratio) + color2.b * ratio;

        // Determine dominant color component
        float maxComponent = Mathf.Max(r, Mathf.Max(g, b));
        if (maxComponent == r)
        {
            r = Mathf.Clamp01(r + ampFactor);
            g = Mathf.Clamp01(g - ampFactor * ratio / 100f);
            b = Mathf.Clamp01(b - ampFactor * ratio / 100f);
        }
        else if (maxComponent == g)
        {
            r = Mathf.Clamp01(r - ampFactor * ratio / 100f);
            g = Mathf.Clamp01(g + ampFactor);
            b = Mathf.Clamp01(b - ampFactor * ratio / 100f);
        }
        else
        {
            r = Mathf.Clamp01(r - ampFactor * ratio / 100f);
            g = Mathf.Clamp01(g - ampFactor * ratio / 100f);
            b = Mathf.Clamp01(b + ampFactor);
        }

        // Return new color with mixed and amplified RGB values
        return new Color(r, g, b);
    }

    public Color Variation(Color color, float factor)
    {
        return new Color(
            ClampRandom(color.r, factor), 
            ClampRandom(color.g, factor), 
            ClampRandom(color.b, factor)
            );
    }

    private float ClampRandom(float value, float factor)
    {
        return Mathf.Clamp01(value * Random.Range(1 - factor, 1 + factor));
    }
}