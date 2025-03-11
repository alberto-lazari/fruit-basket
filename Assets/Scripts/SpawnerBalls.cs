using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerBalls : MonoBehaviour
{
    [SerializeField] private GameObject m_Ball;
    [SerializeField] private float m_Force = 10f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            GameObject activeCamera = Camera.main.gameObject;
            GameObject clone = Instantiate<GameObject>(m_Ball);

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
}
