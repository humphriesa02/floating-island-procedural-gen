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
    [SerializeField, HideInInspector] private GameObject[] skyIslands;
    public GameObject[] SkyIslands => skyIslands;

    /* The Center of the Layout */
    private Vector3 layoutCenter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        layoutCenter = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(layoutCenter, new Vector3(halfWidth * 2, halfHeight * 2, halfLength * 2));
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

    public abstract void GenerateIslands();

}
