using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class BasketFruitCounter : MonoBehaviour
{
    [SerializeField] private SphereCollider m_Collider;
    [SerializeField] private TextMeshProUGUI m_CounterText;

    private int m_LockedFruitCount = 0;
    private int m_DisplayFruitCount = 0;

    private void Start()
    {
        if (m_Collider == null) m_Collider = GetComponent<SphereCollider>();
        if (m_CounterText == null) Debug.LogError("UI Counter text is not assigned");
    }

    private void Update()
    {
        int currentFruits = m_LockedFruitCount;

        // Update UI if fruit count changed
        if (m_DisplayFruitCount != currentFruits && m_CounterText != null)
        {
            m_DisplayFruitCount = currentFruits;
            m_CounterText.text = $"Fruits in Basket: {currentFruits}";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Fruit")) return;
        Interlocked.Increment(ref m_LockedFruitCount);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Fruit")) return;
        Interlocked.Decrement(ref m_LockedFruitCount);
    }
}
