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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Size -= Mathf.Pow(Size + 1, 0.9f) / 50f * Time.deltaTime;
    }
}
