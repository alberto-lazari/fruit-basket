using System.Collections.Generic;

using UnityEngine;

public class BasketFruitCounter : MonoBehaviour
{
    [SerializeField] private SphereCollider m_Collider;

    private HashSet<GameObject> m_Fruits = new();

    public int Fruits()
    {
        return m_Fruits.Count;
    }

    private void Start()
    {
        if (m_Collider == null) m_Collider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        Debug.Log($"Points: {Fruits()}");
    }

    private void OnTriggerEnter(Collider i_Fruit)
    {
        m_Fruits.Add(i_Fruit.gameObject);
    }

    private void OnTriggerExit(Collider i_Fruit)
    {
        m_Fruits.Remove(i_Fruit.gameObject);
    }
}
