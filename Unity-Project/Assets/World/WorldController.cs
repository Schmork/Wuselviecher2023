using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    [SerializeField] Valhalla Valhalla;
    [SerializeField] float ValhallaMutation;
    [SerializeField] Vector2 DecayScoreTimes;

    [SerializeField] Vector2 CellSpawnTimes;
    [SerializeField] float CellSpawnRadius;

    static List<GameObject> _pooledObjects;
    [SerializeField] GameObject CellPrefab;

    private void Awake()
    {
        _pooledObjects = new List<GameObject>();
    }

    void Start()
    {
        Valhalla.RefreshDashboard();
        InvokeRepeating(nameof(Spawn), CellSpawnTimes.x, CellSpawnTimes.y);
        InvokeRepeating(nameof(DecayScores), DecayScoreTimes.x, DecayScoreTimes.y);
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

    void DecayScores()
    {
        Valhalla.DecayScores();
    }

    void Spawn()
    {
        if (1f / Time.unscaledDeltaTime < 60) return;
        var existingCells = FindObjectsOfType<SizeController>();
        Dashboard.UpdateCellCount(existingCells.Length);
        Dashboard.UpdateCellMass(existingCells.Sum(c => c.Size));
        var which = Random.Range(-1, 2) * transform.right * CellSpawnRadius * 2.3f;
        var pos = which + transform.position + (Vector3)Random.insideUnitCircle * CellSpawnRadius;

        SizeController other;
        for (int i = 0; i < existingCells.Length; i++)
        {
            other = existingCells[i];
            if (Vector2.Distance(other.transform.position, pos) < other.Size2Scale())
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
        cell.GetComponent<SizeController>().Size = Random.Range(
            WorldConfig.Instance.CellSizeMin,
            WorldConfig.Instance.CellSizeMax);
        cell.GetComponent<StatsCollector>().Valhalla = Valhalla;

        var mc = cell.GetComponent<MovementController>();
        var hero = Valhalla.GetRandomHero();
        if (Random.value < 0.95 && hero != null)
        {
            mc.SetBrain(new NeuralNetwork(hero, ValhallaMutation));
            if (mc.Brain.generation > oldestGen)
            {
                oldestGen = mc.Brain.generation;
                Dashboard.UpdateCellMaxGen(oldestGen);
            }
        }
        else
            mc.SetBrain(new NeuralNetwork());
        cell.SetActive(true);

        var mcs = FindObjectsOfType<MovementController>();
        if (mcs.Length > 0)
            Dashboard.UpdateCellAvgGen(mcs.Average(c => c.Brain.generation));
    }

    static int oldestGen = 0;
}