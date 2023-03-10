using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Dashboard : MonoBehaviour
{
    static Slider _decaySlider;

    static TMP_Text _distanceTravelledText;
    static TMP_Text _massEatenText;
    static TMP_Text _timeSurvivedText;
    static TMP_Text _massEatenAtSpeedText;
    static TMP_Text _fastestSpeedAchievedText;
    static TMP_Text _massPerActionText;
    static TMP_Text _straightMassText;
    static TMP_Text _cellCountText;
    static TMP_Text _cellMassText;
    static TMP_Text _cellMaxGenText;
    static TMP_Text _cellAvgGenText;

    [SerializeField] TMP_Text distanceTravelledText;
    [SerializeField] TMP_Text massEatenText;
    [SerializeField] TMP_Text timeSurvivedText;
    [SerializeField] TMP_Text massEatenAtSpeedText;
    [SerializeField] TMP_Text fastestSpeedAchievedText;
    [SerializeField] TMP_Text massPerActionText;
    [SerializeField] TMP_Text straightMassText;
    [SerializeField] TMP_Text cellCountText;
    [SerializeField] TMP_Text cellMassText;
    [SerializeField] TMP_Text cellMaxGenText;
    [SerializeField] TMP_Text cellAvgGenText;

    [SerializeField] TMP_Text speedValue;
    [SerializeField] Slider speedSlider;

    [SerializeField] TMP_Text decayValue;
    [SerializeField] Slider decaySlider;

    [SerializeField] TMP_Text fenceValue;
    [SerializeField] Slider fenceSlider;

    [SerializeField]
    Slider
        vhDistanceTravelled,
        vhNumEaten,
        vhMassEaten,
        vhTimeSurvived,
        vhSpeedEaten,
        vhMaxSpeed,
        vhMassPerAction,
        vhStraightMass;

    [SerializeField] Toggle toggleSpeed;

    public static float Decay
    {
        get
        {
            return Mathf.Pow(10, -_decaySlider.value + 1);
        }
    }

    private void Awake()
    {
        var eventTriggerPointerDown = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        eventTriggerPointerDown.callback.AddListener((data) => { toggleSpeed.isOn = false; });
        speedSlider.GetComponent<EventTrigger>().triggers.Add(eventTriggerPointerDown);
        speedSlider.onValueChanged.AddListener((value) =>
        {
            var str = "";
            if (value < 10)
            {
                value /= 10f;
                str = "F1";
            }
            else value -= 9;
            if (value > 15) 
                value = (int)Mathf.Pow(value, 1.2f) - 10;

            Time.timeScale = value;
            speedValue.text = value.ToString(str);
            PlayerPrefs.SetFloat("sim speed", value);
        });

        decaySlider.onValueChanged.AddListener((value) =>
        {
            decayValue.text = Decay.ToString("F7");
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
        vhSpeedEaten.value = PlayerPrefs.GetFloat("chance" + Valhalla.Metric.MassEatenAtSpeed.ToString());
        vhTimeSurvived.value = PlayerPrefs.GetFloat("chance" + Valhalla.Metric.TimeSurvived.ToString());
        vhStraightMass.value = PlayerPrefs.GetFloat("chance" + Valhalla.Metric.StraightMass.ToString());
    }

    void Update()
    {
        if (!toggleSpeed.isOn) return;
        var fps = 1f / Time.unscaledDeltaTime;
        if (fps > 75) speedSlider.value++;
        if (fps < 35 && speedSlider.value >= 10) speedSlider.value--;
    }

    public void VhDistanceTravelledChanged(float value)
    {
        Valhalla.chance[(int)Valhalla.Metric.DistanceTravelled] = value;
        PlayerPrefs.SetFloat("chance" + Valhalla.Metric.DistanceTravelled.ToString(), value);
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

    public static void UpdateDistanceTravelledRecord(float value)
    {
        _distanceTravelledText.text = value.ToString("F2");
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
