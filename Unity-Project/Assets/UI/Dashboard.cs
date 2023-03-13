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
