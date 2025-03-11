using UnityEngine;

public class GravityController : MonoBehaviour
{
    [SerializeField] private float m_GravityScale = 1f;
    [SerializeField] private float m_TimeScale = 1f;

    private static readonly Vector3 m_DefalutGravity = Physics.gravity;

    private void Start()
    {
        Physics.gravity = m_DefalutGravity * m_GravityScale;
        Time.timeScale = m_TimeScale;
    }
}
