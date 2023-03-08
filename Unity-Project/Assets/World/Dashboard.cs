using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

public class Dashboard : MonoBehaviour
{
    private static Slider _decaySlider;

    private static TMP_Text _distanceTravelledText;
    private static TMP_Text _numEatenText;
    private static TMP_Text _massEatenText;
    private static TMP_Text _timeSurvivedText;
    private static TMP_Text _massEatenAtSpeedText;
    private static TMP_Text _fastestSpeedAchievedText;
    private static TMP_Text _massPerActionText;
    private static TMP_Text _straightMassText;
    private static TMP_Text _cellCountText;
    private static TMP_Text _cellMassText;
    private static TMP_Text _cellMaxGenText;
    private static TMP_Text _cellAvgGenText;

    [SerializeField] private TMP_Text distanceTravelledText;
    [SerializeField] private TMP_Text numEatenText;
    [SerializeField] private TMP_Text massEatenText;
    [SerializeField] private TMP_Text timeSurvivedText;
    [SerializeField] private TMP_Text massEatenAtSpeedText;
    [SerializeField] private TMP_Text fastestSpeedAchievedText;
    [SerializeField] private TMP_Text massPerActionText;
    [SerializeField] private TMP_Text straightMassText;
    [SerializeField] private TMP_Text cellCountText;
    [SerializeField] private TMP_Text cellMassText;
    [SerializeField] private TMP_Text cellMaxGenText;
    [SerializeField] private TMP_Text cellAvgGenText;

    [SerializeField] private TMP_Text speedValue;
    [SerializeField] private Slider speedSlider;

    [SerializeField] private TMP_Text decayValue;
    [SerializeField] private Slider decaySlider;

    [SerializeField] private TMP_Text fenceValue;
    [SerializeField] private Slider fenceSlider;

    [SerializeField] private Slider 
        vhDistanceTravelled, 
        vhNumEaten, 
        vhMassEaten, 
        vhTimeSurvived, 
        vhSpeedEaten, 
        vhMaxSpeed, 
        vhMassPerAction,
        vhStraightMass;

    private void Awake()
    {
        speedSlider.onValueChanged.AddListener((value) =>
        {
            Time.timeScale = value;
            speedValue.text = value.ToString("F2");
            PlayerPrefs.SetFloat("sim speed", value);
        });

        decaySlider.onValueChanged.AddListener((value) =>
        {
            decayValue.text = CalcDecay(value).ToString("F7");
            PlayerPrefs.SetFloat("decay", value);
        });

        fenceSlider.onValueChanged.AddListener((value) =>
        {
            var fence = (int)value;
            fenceValue.text = fence.ToString();
            PlayerPrefs.SetInt("fence", fence);
            WorldConfig.Instance.FenceRadius = fence;
        });

        _distanceTravelledText = distanceTravelledText;
        _numEatenText = numEatenText;
        _massEatenText = massEatenText;
        _timeSurvivedText = timeSurvivedText;
        _massEatenAtSpeedText = massEatenAtSpeedText;
        _fastestSpeedAchievedText = fastestSpeedAchievedText;
        _massPerActionText = massPerActionText;
        _straightMassText = straightMassText;
        _cellCountText = cellCountText;
        _cellMassText = cellMassText;
        _cellMaxGenText = cellMaxGenText;
        _cellAvgGenText = cellAvgGenText;

        _decaySlider = decaySlider;

        Time.timeScale = PlayerPrefs.GetFloat("sim speed");
        speedSlider.value = Time.timeScale;
        decaySlider.value = PlayerPrefs.GetFloat("decay");
        fenceSlider.value = PlayerPrefs.GetInt("fence");

        vhDistanceTravelled.value = PlayerPrefs.GetFloat("chance" + Valhalla.Metric.DistanceTravelled.ToString());
        vhMassEaten.value = PlayerPrefs.GetFloat("chance" + Valhalla.Metric.MassEaten.ToString());
        vhMassPerAction.value = PlayerPrefs.GetFloat("chance" + Valhalla.Metric.MassPerAction.ToString());
        vhMaxSpeed.value = PlayerPrefs.GetFloat("chance" + Valhalla.Metric.FastestSpeedAchieved.ToString());
        vhNumEaten.value = PlayerPrefs.GetFloat("chance" + Valhalla.Metric.NumEaten.ToString());
        vhSpeedEaten.value = PlayerPrefs.GetFloat("chance" + Valhalla.Metric.MassEatenAtSpeed.ToString());
        vhTimeSurvived.value = PlayerPrefs.GetFloat("chance" + Valhalla.Metric.TimeSurvived.ToString());
        vhStraightMass.value = PlayerPrefs.GetFloat("chance" + Valhalla.Metric.StraightMass.ToString());
    }

    public void VhDistanceTravelledChanged(float value)
    {
        Valhalla.chance[(int)Valhalla.Metric.DistanceTravelled] = value;
        PlayerPrefs.SetFloat("chance" + Valhalla.Metric.DistanceTravelled.ToString(), value);
    }

    public void VhNumEatenChanged(float value)
    {
        Valhalla.chance[(int)Valhalla.Metric.NumEaten] = value;
        PlayerPrefs.SetFloat("chance" + Valhalla.Metric.NumEaten.ToString(), value);
    }

    public void VhMassEatenChanged(float value)
    {
        Valhalla.chance[(int)Valhalla.Metric.MassEaten] = value;
        PlayerPrefs.SetFloat("chance" + Valhalla.Metric.MassEaten.ToString(), value);
    }

    public void VhTimeSurvivedChanged(float value)
    {
        Valhalla.chance[(int)Valhalla.Metric.TimeSurvived] = value;
        PlayerPrefs.SetFloat("chance" + Valhalla.Metric.TimeSurvived.ToString(), value);
    }

    public void VhSpeedEatenChanged(float value)
    {
        Valhalla.chance[(int)Valhalla.Metric.MassEatenAtSpeed] = value;
        PlayerPrefs.SetFloat("chance" + Valhalla.Metric.MassEatenAtSpeed.ToString(), value);
    }

    public void VhMaxSpeedChanged(float value)
    {
        Valhalla.chance[(int)Valhalla.Metric.FastestSpeedAchieved] = value;
        PlayerPrefs.SetFloat("chance" + Valhalla.Metric.FastestSpeedAchieved.ToString(), value);
    }

    public void VhMassPerActionChanged(float value)
    {
        Valhalla.chance[(int)Valhalla.Metric.MassPerAction] = value;
        PlayerPrefs.SetFloat("chance" + Valhalla.Metric.MassPerAction.ToString(), value);
    }
    public void VhStraightMassChanged(float value)
    {
        Valhalla.chance[(int)Valhalla.Metric.StraightMass] = value;
        PlayerPrefs.SetFloat("chance" + Valhalla.Metric.StraightMass.ToString(), value);
    }

    private static float CalcDecay(float value)
    {
        return Mathf.Pow(10, -value + 1);
    }

    public static float GetDecay()
    {
        return CalcDecay(_decaySlider.value);
    }

    public static void UpdateDistanceTravelledRecord(float value)
    {
        _distanceTravelledText.text = value.ToString("F2");
    }

    public static void UpdateNumEatenRecord(int value)
    {
        _numEatenText.text = value.ToString();
    }

    public static void UpdateMassEatenRecord(float value)
    {
        _massEatenText.text = value.ToString("F2");
    }

    public static void UpdateTimeSurvivedRecord(float value)
    {
        _timeSurvivedText.text = value.ToString("F2");
    }

    public static void UpdateMassEatenAtSpeedRecord(float value)
    {
        _massEatenAtSpeedText.text = value.ToString("F2");
    }

    public static void UpdateFastestSpeedAchievedRecord(float value)
    {
        _fastestSpeedAchievedText.text = value.ToString("F2");
    }

    public static void UpdateMassPerAction(float value)
    {
        _massPerActionText.text = value.ToString("F2");
    }

    public static void UpdateStraightMass(float value)
    {
        _straightMassText.text = value.ToString("F2");
    }

    public static void UpdateCellCount(int value)
    {
        _cellCountText.text = value.ToString();
    }

    public static void UpdateCellMass(float value)
    {
        _cellMassText.text = value.ToString("F2");
    }

    public static void UpdateCellMaxGen(int value)
    {
        _cellMaxGenText.text = value.ToString();
    }

    public static void UpdateCellAvgGen(double value)
    {
        _cellAvgGenText.text = value.ToString("F2");
    }
}
