using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float panSpeed = 0.1f;
    [SerializeField] float zoomSpeed = 0.7f;
    [SerializeField] float minZoom = 5;
    [SerializeField] float maxZoom = 200;

    private Vector3 panStart;
    private Vector3 lastPanPosition;
    private float zoomStart;

    private void Start()
    {
        zoomStart = Camera.main.orthographicSize;
    }

    private void Update()
    {
        float adjustedPanSpeed = panSpeed * Mathf.Pow(zoomStart, 0.3f);

        if (Input.GetMouseButtonDown(0))
        {
            panStart = GetWorldPositionFromScreen(Input.mousePosition);
            lastPanPosition = panStart;
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 currentPanPosition = GetWorldPositionFromScreen(Input.mousePosition);
            Vector3 panOffset = lastPanPosition - currentPanPosition;
            transform.position += panOffset * adjustedPanSpeed;
            lastPanPosition = currentPanPosition;
        }

        float zoomDelta = -Input.mouseScrollDelta.y * zoomSpeed;
        zoomStart = Mathf.Clamp(zoomStart + zoomDelta, minZoom, maxZoom);
        Camera.main.orthographicSize = zoomStart;

        if (Input.GetMouseButtonDown(2))
        {
            transform.position = Vector3.zero;
            zoomStart = (minZoom + maxZoom) / 2;
            Camera.main.orthographicSize = zoomStart;
        }
    }

    private Vector3 GetWorldPositionFromScreen(Vector3 screenPosition)
    {
        return Camera.main.ScreenToWorldPoint(screenPosition);
    }
}