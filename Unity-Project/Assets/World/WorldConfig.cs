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

    [SerializeField] UnityEngine.UI.Slider speedSlider;
    [SerializeField] UnityEngine.UI.Toggle toggleSpeed;
    [SerializeField] float targetCellCount;
    public static float FPS;

    System.Collections.IEnumerator Start()
    {
        while (true)
        {
            FPS = 1 / Time.unscaledDeltaTime;
            if (!toggleSpeed.isOn) yield return new WaitForSeconds(1);
            if (WorldController.CellCount + FPS > 65 + targetCellCount) speedSlider.value++;
            if (WorldController.CellCount + FPS < 40 + targetCellCount && speedSlider.value > 20) speedSlider.value--;
            yield return new WaitForSeconds(1);
        }
    }

    public static float GaussStd;
    public static float FenceRadius;
    public static Vector3 SpawnRect;

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

    [SerializeField] private int autoSaveMinutes;

    public int AutoSaveMinutes
    {
        get { return autoSaveMinutes; }
        set { autoSaveMinutes = value; }
    }
}
