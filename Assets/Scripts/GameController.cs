using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField] private FruitThrower m_Thrower;
    [SerializeField] private UiCameraSetup m_CameraSetup;
    [SerializeField] private float m_CameraGestureDeadZone = 100f;

    private InputActions m_InputActions;
    private List<Vector2> m_GesturePoints = new();
    private bool m_IsDraggingFruit = false;


    private void Awake()
    {
        m_InputActions = new();
        m_InputActions.Game.MoveLeft.performed += ctx => m_CameraSetup.MoveLeft();
        m_InputActions.Game.MoveRight.performed += ctx => m_CameraSetup.MoveRight();
    }

    private void Start()
    {
        if (m_Thrower == null)
        {
            Debug.LogError("Thrower is not assigned");
        }
        if (m_CameraSetup == null)
        {
            Debug.LogError("Camera is not assigned");
        }
    }

    private void Update()
    {
        if (IsMouseGestureActive()) TrackGesture();
    }

    private void OnEnable()
    {
        m_InputActions.Enable();
    }

    private void OnDisable()
    {
        m_InputActions.Disable();
    }

    private static bool IsMouseGestureActive()
    {
        return Input.GetMouseButtonDown(0)
            || Input.GetMouseButton(0)
            || Input.GetMouseButtonUp(0);
    }

    private void TrackGesture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            // Perform raycast to check if the mouse is over a fruit
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Fruit"))
            {
                m_IsDraggingFruit = m_Thrower.OnFruitGrab(hit.collider.gameObject, hit.point);
            }

            // Clear previous gesture points
            m_GesturePoints.Clear();
        }

        // Track mouse movement
        Vector2 point = Input.mousePosition;
        m_GesturePoints.Add(point);
        if (m_IsDraggingFruit) m_Thrower.OnFruitDrag();

        if (Input.GetMouseButtonUp(0)) ProcessGesture();
    }

    private void ProcessGesture()
    {
        int N = 10;
        if (m_GesturePoints.Count < N) return;

        var gesturePoints = m_GesturePoints.GetRange(m_GesturePoints.Count - N, N);
        Vector2 last = gesturePoints[gesturePoints.Count - 1];
        Vector2 first = gesturePoints[0];
        Vector2 gesture = last - first;

        if (m_IsDraggingFruit)
        {
            m_Thrower.OnFruitRelease(gesture, last);
            m_IsDraggingFruit = false;
        }
        // Camera movement gestures
        else if (gesture.x > m_CameraGestureDeadZone) m_CameraSetup.MoveLeft();
        else if (gesture.x < -m_CameraGestureDeadZone) m_CameraSetup.MoveRight();
    }
}
