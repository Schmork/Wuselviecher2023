using UnityEngine;

public class CollisionController : MonoBehaviour
{
    SizeController sc;
    public Rigidbody2D Rb;
    SpriteRenderer rendr;
    ParticleSystem.MainModule psMain;
    StatsCollector stats;

    private void Start()
    {
        sc = GetComponent<SizeController>();
        rendr = GetComponent<SpriteRenderer>();
        stats = GetComponent<StatsCollector>();
        psMain = GetComponent<ParticleSystem>().main;
        psMain.startColor = rendr.material.color;
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (!collider.CompareTag("Edible")) return;

        var size = sc.Size;

        var other = collider.GetComponent<SizeController>();
        if (other.Size > size) return;
        var diff = size / 30f;
        if (other.Size < diff) diff = other.Size;

        var col1 = rendr.material.color;
        var col2 = other.GetComponent<SpriteRenderer>().material.color;
        var col = MixColors(col1, col2, diff / size);
        rendr.material.color = col;
        psMain.startColor = col;

        sc.Size += diff;
        other.Size -= diff;
        
        stats.MassEatenAtSpeed += 0.1f + diff / sc.Size * (0.01f + Mathf.Sqrt(Rb.velocity.magnitude * Time.deltaTime));
        stats.NumEaten++;
        stats.MassEaten += diff / sc.Size;
        stats.StraightMass += diff / sc.Size / Mathf.Exp(Mathf.Abs(Rb.angularVelocity * Time.deltaTime));

        var otherCc = collider.GetComponent<CollisionController>();
        if (otherCc == null) return;
        Rb.AddForce(otherCc.Rb.velocity * other.Size * Time.deltaTime);
        otherCc.Rb.AddForce(Rb.velocity * size * Time.deltaTime);
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
        var r = ClampRandom(color.r, factor);
        var g = ClampRandom(color.g, factor);
        var b = ClampRandom(color.b, factor);
        return new Color(r, g, b);
    }

    private float ClampRandom(float value, float factor)
    {
        return Mathf.Clamp01(value * Random.Range(1 - factor, 1 + factor));
    }
}
