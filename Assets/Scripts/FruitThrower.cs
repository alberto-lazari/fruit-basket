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
        if (m_CurrentFruit != null && Input.GetKeyDown(KeyCode.W))
        {
            ThrowFruit();
            Invoke(nameof(SpawnFruit), m_SpawnTime);
        }
    }

    private void ThrowFruit()
    {
        Vector3 deviation = new Vector3(
            Random.Range(-.5f, .5f),
            Random.Range(.5f, 1.5f),
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
    }

    private void SpawnFruit()
    {
        GameObject fruit = Random.Range(0, 5) == 0 ? m_Banana : m_Apple;
        m_CurrentFruit = Instantiate<GameObject>(
            fruit,
            transform.position + transform.forward * 10,
            fruit.transform.rotation
        );
        m_CurrentFruit.GetComponent<Rigidbody>().useGravity = false;

        m_CurrentFruit.SetActive(true);
    }
}
