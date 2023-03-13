using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Dashboard : MonoBehaviour
{
    static Slider _decaySlider;

    static TMP_Text _cellCountText;
    static TMP_Text _cellMassText;
    static TMP_Text _cellMaxGenText;
    static TMP_Text _cellAvgGenText;

    [SerializeField] TMP_Text distanceTravelledText;
    [SerializeField] TMP_Text massEatenText;
    [SerializeField] TMP_Text timeSurvivedText;
    [SerializeField] TMP_Text massEatenAtSpeedText;
    [SerializeField] TMP_Text averageSpeedText;
    [SerializeField] TMP_Text massPerActionText;
    [SerializeField] TMP_Text straightMassText;
    [SerializeField] TMP_Text cellCountText;
    [SerializeField] TMP_Text cellMassText;
    [SerializeField] TMP_Text cellMaxGenText;
    [SerializeField] TMP_Text cellAvgGenText;

    [SerializeField] TMP_Text peaceValue;
    [SerializeField] Slider peaceSlider;

    [SerializeField] TMP_Text speedValue;
    [SerializeField] Slider speedSlider;

    [SerializeField] TMP_Text decayValue;
    [SerializeField] Slider decaySlider;

    [SerializeField] TMP_Text fenceValue;
    [SerializeField] Slider fenceSlider;

    [SerializeField] TMP_Text gaussValue;
    [SerializeField] Slider gaussSlider;

    [SerializeField] TMP_Text areaValue;
    [SerializeField] RectangleSlider areaSliderRect;
    [SerializeField] Slider areaSlider;

    [SerializeField]
    Slider
        vhDistanceTravelled,
        vhMassEaten,
        vhTimeSurvived,
        vhSpeedEaten,
        vhAverageSpeed,
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
            PlayerPrefs.SetFloat("Sim Speed", value);
        });

        decaySlider.onValueChanged.AddListener((value) =>
        {
            decayValue.text = Decay.ToString("F7");
            PlayerPrefs.SetFloat("Scores Decay", value);
        });

        fenceSlider.onValueChanged.AddListener((value) =>
        {
            fenceValue.text = value.ToString("F1");
            PlayerPrefs.SetFloat("Sim Fence", value);
            WorldConfig.FenceRadius = value;
        });

        gaussSlider.onValueChanged.AddListener((value) =>
        {
            PlayerPrefs.SetFloat("Sim Gauss", value);
            if (value < 10)
            {
                value /= 10;
            }
            else value -= 9;
            if (value < 10)
            {
                value /= 10;
            }
            else value -= 9;
            WorldConfig.GaussStd = value;
            gaussValue.text = value.ToString("F4");
        });

        peaceSlider.onValueChanged.AddListener((value) =>
        {
            Valhalla.Heroes[Valhalla.Metric.PeaceTime].ChanceToBePicked = value;
            PlayerPrefs.SetFloat("Hero Chance " + Valhalla.Metric.PeaceTime.ToString(), value);
        });

        areaSliderRect.AddOnValueChangedListener((value) =>
        {
            WorldConfig.SpawnRect.x = value.x;
            WorldConfig.SpawnRect.y = value.y;
            PlayerPrefs.SetFloat("Sim SpawnX", value.x);
            PlayerPrefs.SetFloat("Sim SpawnY", value.y);
            UpdateAreaText();
        });

        areaSlider.onValueChanged.AddListener((value) =>
        {
            WorldConfig.SpawnRect.z = value;
            PlayerPrefs.SetFloat("Sim SpawnZ", value);
            UpdateAreaText();
        });

        Valhalla.OnHighscoreChanged += HighscoreChangedHandler;

        _cellCountText = cellCountText;
        _cellMassText = cellMassText;
        _cellMaxGenText = cellMaxGenText;
        _cellAvgGenText = cellAvgGenText;

        _decaySlider = decaySlider;

        speedSlider.value = PlayerPrefs.GetFloat("Sim Speed");
        decaySlider.value = PlayerPrefs.GetFloat("Scores Decay");
        fenceSlider.value = PlayerPrefs.GetFloat("Sim Fence");
        gaussSlider.value = PlayerPrefs.GetFloat("Sim Gauss");
        var width = PlayerPrefs.GetFloat("Sim SpawnX");
        var height = PlayerPrefs.GetFloat("Sim SpawnY");
        areaSliderRect.SetValue(width, height);
        areaSlider.value = PlayerPrefs.GetFloat("Sim SpawnZ");

        vhDistanceTravelled.value = PlayerPrefs.GetFloat("Hero Chance " + Valhalla.Metric.DistanceTravelled.ToString());
        vhMassEaten.value = PlayerPrefs.GetFloat("Hero Chance " + Valhalla.Metric.MassEaten.ToString());
        vhMassPerAction.value = PlayerPrefs.GetFloat("Hero Chance " + Valhalla.Metric.MassPerAction.ToString());
        vhAverageSpeed.value = PlayerPrefs.GetFloat("Hero Chance " + Valhalla.Metric.AverageSpeed.ToString());
        vhSpeedEaten.value = PlayerPrefs.GetFloat("Hero Chance " + Valhalla.Metric.MassEatenAtSpeed.ToString());
        vhTimeSurvived.value = PlayerPrefs.GetFloat("Hero Chance " + Valhalla.Metric.TimeSurvived.ToString());
        vhStraightMass.value = PlayerPrefs.GetFloat("Hero Chance " + Valhalla.Metric.StraightMass.ToString());
        peaceSlider.value = PlayerPrefs.GetFloat("Hero Chance " + Valhalla.Metric.PeaceTime.ToString());
    }

    void UpdateAreaText()
    {
        var area = areaSliderRect.GetValue() * areaSlider.value;
        areaValue.text = area.x.ToString("F0") + ", " + area.y.ToString("F0");
    }

    void HighscoreChangedHandler(Valhalla.Metric metric, float score)
    {
        TMP_Text component = metric switch
        {
            Valhalla.Metric.DistanceTravelled => distanceTravelledText,
            Valhalla.Metric.AverageSpeed => averageSpeedText,
            Valhalla.Metric.MassEaten => massEatenText,
            Valhalla.Metric.MassEatenAtSpeed => massEatenAtSpeedText,
            Valhalla.Metric.MassPerAction => massPerActionText,
            Valhalla.Metric.PeaceTime => peaceValue,
            Valhalla.Metric.StraightMass => straightMassText,
            Valhalla.Metric.TimeSurvived => timeSurvivedText,
            _ => throw new System.NotImplementedException()
        };
        component.text = score.ToString("F2");
    }

    public void VhDistanceTravelledChanged(float value)
    {
        Valhalla.Heroes[Valhalla.Metric.DistanceTravelled].ChanceToBePicked = value;
        PlayerPrefs.SetFloat("Hero Chance " + Valhalla.Metric.DistanceTravelled.ToString(), value);
    }

    public void VhMassEatenChanged(float value)
    {
        Valhalla.Heroes[Valhalla.Metric.MassEaten].ChanceToBePicked = value;
        PlayerPrefs.SetFloat("Hero Chance " + Valhalla.Metric.MassEaten.ToString(), value);
    }

    public void VhTimeSurvivedChanged(float value)
    {
        Valhalla.Heroes[Valhalla.Metric.TimeSurvived].ChanceToBePicked = value;
        PlayerPrefs.SetFloat("Hero Chance " + Valhalla.Metric.TimeSurvived.ToString(), value);
    }

    public void VhSpeedEatenChanged(float value)
    {
        Valhalla.Heroes[Valhalla.Metric.MassEatenAtSpeed].ChanceToBePicked = value;
        PlayerPrefs.SetFloat("Hero Chance " + Valhalla.Metric.MassEatenAtSpeed.ToString(), value);
    }

    public void VhAverageSpeedChanged(float value)
    {
        Valhalla.Heroes[Valhalla.Metric.AverageSpeed].ChanceToBePicked = value;
        PlayerPrefs.SetFloat("Hero Chance " + Valhalla.Metric.AverageSpeed.ToString(), value);
    }

    public void VhMassPerActionChanged(float value)
    {
        Valhalla.Heroes[Valhalla.Metric.MassPerAction].ChanceToBePicked = value;
        PlayerPrefs.SetFloat("Hero Chance " + Valhalla.Metric.MassPerAction.ToString(), value);
    }

    public void VhStraightMassChanged(float value)
    {
        Valhalla.Heroes[Valhalla.Metric.StraightMass].ChanceToBePicked = value;
        PlayerPrefs.SetFloat("Hero Chance " + Valhalla.Metric.StraightMass.ToString(), value);
    }

    public static void UpdateCellCount(int value)
    {
        _cellCountText.text = value.ToString();
    }

    public static void UpdateCellMass(float value)
    {
        _cellMassText.text = value.ToString("F0");
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
