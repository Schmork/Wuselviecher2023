using UnityEngine;

public class WorldConfig : MonoBehaviour
{
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

    public static float GaussStd;

    [SerializeField] private float cellSpawnSize;
    public float CellSpawnSize
    {
        get { return cellSpawnSize; }
        set { cellSpawnSize = value; }
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
