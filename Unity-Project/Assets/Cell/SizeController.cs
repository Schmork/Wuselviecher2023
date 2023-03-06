using UnityEngine;

public class SizeController : MonoBehaviour
{
    Rigidbody2D rb;

    [SerializeField] float size;
    public float Size
    {
        get { return size; }
        set
        {
            //Debug.Log("Size: " + value);
            if (value < WorldConfig.Instance.DeathBelowSize)
            {
                Die();
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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (Size <= 0.01f) Die();
    }

    private void Update()
    {
        Size -= Mathf.Pow(Size + 1, 0.9f) / 50f * Time.deltaTime;
    }

    private void Die()
    {
        var stats = GetComponent<StatsCollector>();
        if (stats != null) stats.AddToValhalla(GetComponent<MovementController>().Brain);
        Destroy(gameObject);
    }
}
