using UnityEngine;

public class FruitThrower : MonoBehaviour
{
    [SerializeField] private GameObject m_Apple;
    [SerializeField] private GameObject m_Banana;
    [SerializeField] private float m_Force = 25f;

    private System.Random m_Rand = new System.Random();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            GameObject fruit = m_Rand.Next(5) == 0 ? m_Banana : m_Apple;
            Throw(fruit);
        }
    }

    private void Throw(in GameObject i_Fruit)
    {
        GameObject activeCamera = Camera.main.gameObject;
        GameObject clone = Instantiate<GameObject>(i_Fruit);

        clone.transform.position = activeCamera.transform.position;

        clone.SetActive(true);

        Vector3 deviation = new Vector3(
                Random.Range(-.5f, .5f), Random.Range(.5f, 1.5f), 1);
        clone.GetComponent<Rigidbody>().AddForce(
                activeCamera.transform.rotation
                * deviation
                * m_Force, ForceMode.Impulse);
    }
}
