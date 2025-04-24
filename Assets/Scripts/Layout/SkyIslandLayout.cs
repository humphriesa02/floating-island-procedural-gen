using System.ComponentModel;
using UnityEngine;

public abstract class SkyIslandLayout : MonoBehaviour
{

    [Header("Island Layout Connections")]
    [SerializeField] protected GameObject entryPoint;
    [SerializeField] protected GameObject exitPoint;
    [SerializeField] protected GameObject leftPoint;
    [SerializeField] protected GameObject rightPoint;

    [Header("Island Layout Bounds")]
    [SerializeField] private float halfWidth;
    [SerializeField] private float halfLength;
    [SerializeField] private float halfHeight;

    [Header("Layout Links")]
    [SerializeField] private SkyIslandLayout previousLayout { get; set; }
    [SerializeField] private SkyIslandLayout nextLayout { get; set; }

    [Header("SkyIslands (Generated)")]
    [SerializeField, HideInInspector] protected GameObject[] skyIslands;
    public GameObject[] SkyIslands => skyIslands;

    /* The Center of the Layout */
    private Vector3 layoutCenter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void Awake()
    {
        layoutCenter = gameObject.transform.position;

        entryPoint.transform.localPosition = new Vector3(0, 0, -halfLength);
        exitPoint.transform.localPosition = new Vector3(0, 0, halfLength);
        leftPoint.transform.localPosition = new Vector3(-halfWidth, 0, 0);
        rightPoint.transform.localPosition = new Vector3(halfWidth, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        // Save the current Gizmos matrix
        Matrix4x4 oldMatrix = Gizmos.matrix;

        // Set Gizmos matrix to match the object's transform (position + rotation)
        Gizmos.matrix = transform.localToWorldMatrix;

        // Draw the wire cube at the origin of the transform (local position (0,0,0))
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(halfWidth * 2, halfHeight * 2, halfLength * 2));

        // Restore the previous matrix
        Gizmos.matrix = oldMatrix;

        Gizmos.color = Color.green;
        if (entryPoint) Gizmos.DrawSphere(entryPoint.transform.position, 0.5f);
        if (exitPoint) Gizmos.DrawSphere(exitPoint.transform.position, 0.5f);

        Gizmos.color = Color.red;
        if (leftPoint) Gizmos.DrawSphere(leftPoint.transform.position, 0.5f);
        if (rightPoint) Gizmos.DrawSphere(rightPoint.transform.position, 0.5f);
    }

    public void SetConnectionPoints(GameObject entry, GameObject exit, GameObject left, GameObject right)
    {
        entryPoint = entry;
        exitPoint = exit;
        leftPoint = left;
        rightPoint = right;
    }

    public float getHalfWidth() { return halfWidth; }
    public float getHalfLength() { return halfLength; }
    public float getHalfHeight() { return halfHeight; }
    public GameObject getEntryPoint() { return entryPoint; }
    public GameObject getExitPoint() {  return exitPoint; }
    public GameObject getLeftPoint() {  return leftPoint; }
    public GameObject getRightPoint() {  return rightPoint; }
    public void SetPreviousLayout(SkyIslandLayout layout)
    {
        previousLayout = layout;
    }
    public void SetNextLayout(SkyIslandLayout layout)
    {
        nextLayout = layout;
    }

    public SkyIslandLayout GetPreviousLayout()
    {
        return previousLayout;
    }

    public SkyIslandLayout GetNextLayout()
    {
        return nextLayout;
    }

    public abstract void GenerateIslands();

    public GameObject[] getSkyIslands()
    {
        return SkyIslands;
    }

    private void CreateOrMovePoint(ref GameObject point, string name, Vector3 localOffset)
    {
        if (point == null)
        {
            point = new GameObject(name);
            point.transform.parent = this.transform;
        }
        point.name = name; // Keep it updated even if manually assigned
        point.transform.localPosition = localOffset;
        point.transform.localRotation = Quaternion.identity;
    }
}
