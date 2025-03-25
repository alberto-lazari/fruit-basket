using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField] private FruitThrower m_Thrower;
    [SerializeField] private UiCameraSetup m_CameraSetup;
    [SerializeField] private int m_GesturePointsNumber = 8;
    [SerializeField] private float m_CameraGestureDeadZone = 0.05f;

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
        if (IsMouseGestureActive())
        {
            if (Input.GetMouseButtonDown(0)) OnGestureBegin();
            if (m_IsDraggingFruit) m_Thrower.OnFruitDrag(MousePosition());
            if (Input.GetMouseButtonUp(0)) OnGestureEnd();
        }
    }

    // Graphics frames are not reliable since they might skip points
    private void FixedUpdate()
    {
        m_GesturePoints.Add(MousePosition());
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

    /**
     * Return viewport mouse position, normalized over the current screen size
     */
    private Vector2 MousePosition()
    {
        return new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
        );
    }

    private void OnGestureBegin()
    {
        // Perform raycast to check if the mouse is over a fruit
        Ray ray = Camera.main.ViewportPointToRay(MousePosition());
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Fruit"))
        {
            m_IsDraggingFruit = m_Thrower.OnFruitGrab(hit.collider.gameObject, hit.point);
        }

        // Clear previous points and begin new gesture
        m_GesturePoints.Clear();
        m_GesturePoints.Add(MousePosition());
    }

    private void OnGestureEnd()
    {
        m_GesturePoints.Add(MousePosition());

        Vector2 first = m_GesturePoints.Count > m_GesturePointsNumber
            ? m_GesturePoints[m_GesturePoints.Count - m_GesturePointsNumber]
            : m_GesturePoints[0];
        Vector2 last = m_GesturePoints.Count > m_GesturePointsNumber
            ? m_GesturePoints[m_GesturePoints.Count - 1]
            // Generate a null gesture if lenght is insufficient
            : first;
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
