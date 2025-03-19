using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField] private FruitThrower m_Thrower;
    [SerializeField] private CameraSetup m_3dCameraSetup;
    [SerializeField] private CameraSetup m_UiCameraSetup;

    private InputActions m_InputActions;

    private void Awake()
    {
        m_InputActions = new InputActions();
        m_InputActions.Game.Throw.performed += ctx => m_Thrower.ThrowFruit();
        m_InputActions.Game.MoveLeft.performed += ctx =>
        {
            m_3dCameraSetup.MoveLeft();
            m_UiCameraSetup.MoveLeft();
        };
        m_InputActions.Game.MoveRight.performed += ctx =>
        {
            m_3dCameraSetup.MoveRight();
            m_UiCameraSetup.MoveRight();
        };
    }

    private void OnEnable()
    {
        m_InputActions.Enable();
    }

    private void OnDisable()
    {
        m_InputActions.Disable();
    }
}
