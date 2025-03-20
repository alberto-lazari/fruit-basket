using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField] private FruitThrower m_Thrower;
    [SerializeField] private UiCameraSetup m_CameraSetup;

    private List<Vector2> m_ThrowGesturePoints = new List<Vector2>();

    private InputActions m_InputActions;

    private void Awake()
    {
        m_InputActions = new InputActions();
        m_InputActions.Game.MoveLeft.performed += ctx => m_CameraSetup.MoveLeft();
        m_InputActions.Game.MoveRight.performed += ctx => m_CameraSetup.MoveRight();
    }

    private void Start()
    {
        if (m_Thrower == null) Debug.LogError("Thrower is not assigned");
        if (m_CameraSetup == null) Debug.LogError("Camera is not assigned");
    }

    private void Update()
    {
        TrackThrowGesture();
    }

    private void OnEnable()
    {
        m_InputActions.Enable();
    }

    private void OnDisable()
    {
        m_InputActions.Disable();
    }

    private void TrackThrowGesture()
    {
        if (!( Input.GetMouseButtonDown(0)
            || Input.GetMouseButton(0)
            || Input.GetMouseButtonUp(0) )
        ) return;

        m_ThrowGesturePoints.Add(Input.mousePosition);

        if (Input.GetMouseButtonDown(0)) m_ThrowGesturePoints.Clear();
        else if (Input.GetMouseButtonUp(0)) ProcessThrowGesture();
    }

    private void ProcessThrowGesture()
    {
        if (m_ThrowGesturePoints.Count < 2) return;

        Vector2 last = m_ThrowGesturePoints[m_ThrowGesturePoints.Count - 1];
        Vector2 first = m_ThrowGesturePoints[0];
        Vector2 gesture = last - first;

        m_Thrower.ThrowFruit(gesture);
    }
}
