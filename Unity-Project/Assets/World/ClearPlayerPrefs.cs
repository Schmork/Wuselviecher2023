using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ClearPlayerPrefs : MonoBehaviour
{
#if UNITY_EDITOR
    [CustomEditor(typeof(ClearPlayerPrefs))]
    public class ClearPlayerPrefsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Clear Valhalla"))
            {
                foreach (var metric in System.Enum.GetValues(typeof(Valhalla.Metric)))
                {
                    PlayerPrefs.DeleteKey(Valhalla.VHERO + metric.ToString());
                    PlayerPrefs.DeleteKey(Valhalla.VSCOR + metric.ToString());
                }
                Debug.Log("Valhalla cleared.");
            }
        }
    }
#endif
}
