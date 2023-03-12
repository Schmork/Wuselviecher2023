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
        var sizeTime  = Mathf.Pow(Size + 1, 1.1f) * Time.deltaTime;
        var slow = (1 + rb.velocity.magnitude);
        Size -= sizeTime / 1000f;
        Size -= sizeTime / 1500f / slow * Mathf.Abs(rb.angularVelocity);
        Size -= sizeTime / 500f / slow;
    }
}
