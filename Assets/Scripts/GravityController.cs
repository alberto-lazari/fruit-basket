using UnityEngine;

public class GravityController : MonoBehaviour
{
    [SerializeField] private float m_GravityScale = 1f;
    [SerializeField] private float m_TimeScale = 1f;

    private void Start()
    {
        Physics.gravity = Physics.gravity * m_GravityScale;
        Time.timeScale = m_TimeScale;
    }
}
