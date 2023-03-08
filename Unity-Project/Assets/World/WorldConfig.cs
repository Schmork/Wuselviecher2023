using UnityEngine;

public class WorldConfig : MonoBehaviour
{
    private static System.Random random;
    public static System.Random Random
    {
        get
        {
            if (random == null)
            {
                random = new System.Random();
            }
            return random;
        }
    }

    private static WorldConfig instance = null;

    public static WorldConfig Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("WorldConfig").AddComponent<WorldConfig>();
                Debug.Log("New Instance of WorldConfig created.");
            }
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private float initialValues;
    public float InitialValues
    {
        get { return initialValues; }
        set { initialValues = value; }
    }

    [SerializeField] private float cellSizeMin;
    public float CellSizeMin
    {
        get { return cellSizeMin; }
        set { cellSizeMin = value; }
    }

    [SerializeField] private float cellSizeMax;
    public float CellSizeMax
    {
        get { return cellSizeMax; }
        set { cellSizeMax = value; }
    }

    [SerializeField] private float deathBelowSize;

    public float DeathBelowSize
    {
        get { return deathBelowSize; }
        set { deathBelowSize = value; }
    }

    [SerializeField] private float fenceRadius;

    public float FenceRadius
    {
        get { return fenceRadius; }
        set { fenceRadius = value; }
    }
}
