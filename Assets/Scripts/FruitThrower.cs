using UnityEngine;

public class FruitThrower : MonoBehaviour
{
    [SerializeField] private GameObject m_Apple;
    [SerializeField] private GameObject m_Banana;
    [SerializeField] private float m_ForceModifier = 15f;
    [SerializeField] private float m_Torque = 2f;
    [SerializeField] private float m_SpawnDistance = 20f;
    [SerializeField] private float m_SpawnTime = 2f;

    private GameObject m_CurrentFruit;
    private Vector3 m_HitOffset;
    private bool m_IsDragging = false;
    private Plane m_DragPlane;

    private void Start()
    {
        GameObject camera = Camera.main.gameObject;
        transform.position = camera.transform.position;
        transform.rotation = camera.transform.rotation;

        SpawnFruit();
    }

    private void Update()
    {
        if (!m_IsDragging) AdjustFruitPosition();
    }

    private Vector3 FruitPosition()
    {
        return transform.position + transform.forward * m_SpawnDistance;
    }

    private void AdjustFruitPosition()
    {
        if (m_CurrentFruit == null) return;

        GameObject camera = Camera.main.gameObject;
        if (m_CurrentFruit.transform.position == camera.transform.position) return;

        transform.position = camera.transform.position;
        transform.rotation = camera.transform.rotation;

        m_CurrentFruit.transform.position = FruitPosition();
        m_CurrentFruit.transform.rotation = transform.rotation;
    }

    private void SpawnFruit()
    {
        GameObject fruit = Random.Range(0, 5) == 0 ? m_Banana : m_Apple;
        m_CurrentFruit = Instantiate<GameObject>(
            fruit,
            FruitPosition(),
            transform.rotation
        );
        m_CurrentFruit.GetComponent<Rigidbody>().useGravity = false;

        m_CurrentFruit.SetActive(true);
    }


    /**
     * Start dragging by recording the offset.
     */
    public bool OnFruitGrab(GameObject fruit, Vector3 grabPoint)
    {
        if (m_IsDragging || m_CurrentFruit == null || fruit != m_CurrentFruit)
        {
            return m_IsDragging = false;
        }

        // Store the offset in local space
        m_HitOffset = transform.InverseTransformPoint(grabPoint);

        // Define a plane at the same Z position of the thrower
        m_DragPlane = new Plane(transform.forward, m_CurrentFruit.transform.position);
        return m_IsDragging = true;
    }

    /**
     * Update position to keep the hit point aligned with the cursor.
     */
    public void OnFruitDrag()
    {
        if (!m_IsDragging || m_CurrentFruit == null) return;

        // Project onto the dragging plane
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        m_DragPlane.Raycast(ray, out float enter);
        Vector3 newLocalPoint = transform.InverseTransformPoint(ray.GetPoint(enter));

        // Keep Z unchanged and apply the original offset
        newLocalPoint.z = m_HitOffset.z;
        m_CurrentFruit.transform.position = transform.TransformPoint(newLocalPoint);
    }

    /**
     * Throw the current fruit with a force based on the gesture.
     */
    public void OnFruitRelease(Vector2 gesture, Vector2 releasePoint)
    {
        if (m_CurrentFruit == null) return;

        float releaseX = Mathf.Abs(releasePoint.x - 1 / 2);
        Vector3 deviation = new Vector3(
            gesture.x + (Mathf.Sqrt(releaseX) * gesture.x * m_ForceModifier),
            Mathf.Sqrt(releasePoint.y) * gesture.y * m_ForceModifier,
            Mathf.Abs(gesture.y)
        );
        Vector3 force = transform.rotation * deviation;
        force *= m_ForceModifier;
        // Debug.Log($"gesture: {gesture}, deviation: {deviation}, force: {force}");

        Rigidbody rb = m_CurrentFruit.GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(
            transform.right * m_Torque,
            ForceMode.Impulse
        );

        // Detach the fruit from gesture
        m_IsDragging = false;
        m_CurrentFruit = null;

        // Schedule a new fruit spawn
        Invoke(nameof(SpawnFruit), m_SpawnTime);
    }
}
