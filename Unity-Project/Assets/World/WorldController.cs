using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    [SerializeField] Vector2 CellSpawnTimes;
    [SerializeField] float CellSpawnRadius;

    static List<GameObject> _pooledObjects;
    [SerializeField] GameObject CellPrefab;

    static double avgGen;

    private void Awake()
    {
        _pooledObjects = new List<GameObject>();
    }

    void Start()
    {
        InvokeRepeating(nameof(Spawn), CellSpawnTimes.x, CellSpawnTimes.y);
    }

    GameObject GetPooledCell()
    {
        var cell = _pooledObjects.Find(o => !o.activeInHierarchy);
        return cell == null ? MakeNew() : cell;
    }

    GameObject MakeNew()
    {
        var tmp = Instantiate(CellPrefab, transform);
        tmp.SetActive(false);
        _pooledObjects.Add(tmp);
        return tmp;
    }

    void Spawn()
    {
        var existingCells = FindObjectsOfType<SizeController>();
        Dashboard.UpdateCellCount(existingCells.Length);
        Dashboard.UpdateCellMass(existingCells.Sum(c => c.Size));
        for (int t = 0; t < Mathf.Max(1f, 1f / Time.deltaTime / 60f) * Time.unscaledDeltaTime; t++)
        {
            var which = Random.Range(-1, 2) * transform.right * CellSpawnRadius * 2.3f;
            var pos = which + transform.position + (Vector3)Random.insideUnitCircle * CellSpawnRadius;

            SizeController other;
            for (int i = 0; i < existingCells.Length; i++)
            {
                other = existingCells[i];
                if (Vector2.Distance(other.transform.position, pos) < other.Size2Scale() * 1.3f)
                    return;
            }

            var cell = GetPooledCell();
            cell.transform.position = pos;
            cell.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            cell.GetComponent<SpriteRenderer>().material.color =
                Random.ColorHSV(
                0, 1,
                0.6f, 1,
                0.6f, 1,
                1, 1);
            cell.GetComponent<SizeController>().Size = WorldConfig.Instance.CellSpawnSize;

            var mc = cell.GetComponent<MovementController>();
            var hero = Valhalla.GetRandomHero();
            if (Random.value < 1 - 1 / (20 + avgGen * avgGen) && hero != null)
            {
                mc.Brain = new NeuralNetwork(hero, WorldConfig.GaussMean);
                if (mc.Brain.generation > Valhalla.OldestGen)
                {
                    Valhalla.OldestGen = mc.Brain.generation;
                    Dashboard.UpdateCellMaxGen(Valhalla.OldestGen);
                }
            }
            else
                mc.Brain = NeuralNetwork.NewRandom();
            cell.SetActive(true);

            var mcs = FindObjectsOfType<MovementController>();
            if (mcs.Length > 0)
            {
                avgGen = mcs.Average(c => c.Brain.generation);
                Dashboard.UpdateCellAvgGen(avgGen);
            }
        }
    }
}