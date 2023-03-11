using UnityEngine;

public class SizeController : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;

    [SerializeField] float size;
    public float Size
    {
        get { return size; }
        set
        {
            if (value < WorldConfig.Instance.DeathBelowSize)
            {
                gameObject.SetActive(false);
                return;
            }
            size = value;
            transform.localScale = Vector3.one * Size2Scale();

            if (rb != null) rb.mass = value;
        }
    }

    public float Size2Scale()
    {
        return Mathf.Pow(Size, 0.5f);
    }

    void Update()
    {
        Size -= Time.deltaTime / 100f * Mathf.Pow(Size + 1, 0.95f);
        Size -= Time.deltaTime / 500f * Mathf.Abs(rb.angularVelocity) / (1 + rb.velocity.magnitude);
        Size -= Time.deltaTime / 30f / (1 + rb.velocity.magnitude);
    }
}
