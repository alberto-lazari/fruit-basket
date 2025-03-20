using UnityEngine;

public class FruitThrower : MonoBehaviour
{
    [SerializeField] private GameObject m_Apple;
    [SerializeField] private GameObject m_Banana;
    [SerializeField] private float m_Force = 25f;
    [SerializeField] private float m_Torque = 2f;
    [SerializeField] private float m_SpawnTime = 2f;

    private GameObject m_CurrentFruit;

    private void Start()
    {
        GameObject camera = Camera.main.gameObject;
        transform.position = camera.transform.position;
        transform.rotation = camera.transform.rotation;

        SpawnFruit();
    }

    private void Update()
    {
        AdjustFruitPosition();
    }

    private Vector3 FruitPosition()
    {
        return transform.position + transform.forward * 10;
    }

    private void AdjustFruitPosition()
    {
        if (m_CurrentFruit == null) return;

        GameObject camera = Camera.main.gameObject;
        if (m_CurrentFruit.transform.position == camera.transform.position) return;

        transform.position = camera.transform.position;
        transform.rotation = camera.transform.rotation;
        m_CurrentFruit.transform.position = FruitPosition();
    }

    private void SpawnFruit()
    {
        GameObject fruit = Random.Range(0, 5) == 0 ? m_Banana : m_Apple;
        m_CurrentFruit = Instantiate<GameObject>(
            fruit,
            FruitPosition(),
            fruit.transform.rotation
        );
        m_CurrentFruit.GetComponent<Rigidbody>().useGravity = false;

        m_CurrentFruit.SetActive(true);
    }

    public void ThrowFruit(Vector2 i_Gesture)
    {
        if (m_CurrentFruit == null) return;

        Debug.Log(i_Gesture);
        Vector3 deviation = new Vector3(
            i_Gesture.x,
            i_Gesture.y,
            1
        );
        Rigidbody rb = m_CurrentFruit.GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.AddForce(
            transform.rotation * deviation * m_Force,
            ForceMode.Impulse
        );
        rb.AddTorque(
            transform.right * m_Torque,
            ForceMode.Impulse
        );

        m_CurrentFruit = null;
        Invoke(nameof(SpawnFruit), m_SpawnTime);
    }
}
