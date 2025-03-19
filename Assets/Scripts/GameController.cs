using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField] private FruitThrower m_Thrower;
    [SerializeField] private UiCameraSetup m_CameraSetup;

    private InputActions m_InputActions;

    private void Awake()
    {
        m_InputActions = new InputActions();
        m_InputActions.Game.Throw.performed += ctx => m_Thrower.ThrowFruit();
        m_InputActions.Game.MoveLeft.performed += ctx => m_CameraSetup.MoveLeft();
        m_InputActions.Game.MoveRight.performed += ctx => m_CameraSetup.MoveRight();
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
