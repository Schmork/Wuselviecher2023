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

            if (GUILayout.Button("Clear PlayerPrefs"))
            {
                PlayerPrefs.DeleteAll();
                Debug.Log("PlayerPrefs cleared.");
            }
        }
    }
#endif
}
