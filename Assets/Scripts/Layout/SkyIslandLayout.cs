using System.ComponentModel;
using UnityEngine;
using UnityEngine.SocialPlatforms;

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

    public void GradiantSeparation(int iterations = 20, float repulsionStrength = 1.0f, float boundaryRepulsion = 0.5f)
    {

        //Ensure there are islands to separate
        if (skyIslands == null || skyIslands.Length == 0) return;

        //For as many iterations as specified
        for (int iter = 0; iter < iterations; iter++) {

            Vector3[] adjustments = new Vector3[skyIslands.Length];

            //iterate over the islands
            for (int i = 0; i < skyIslands.Length; i++)
            {

                //Get and confirm Island
                GameObject currIsland = skyIslands[i];
                if(currIsland == null) continue;

                //Get Island Data
                Vector3 currPos = currIsland.transform.localPosition;
                float currRadius = currIsland.GetComponent<GenerateIsland>().GetRadius();

                Vector3 totalRepulsion = Vector3.zero;

                //iterate over the islands again
                for (int j = 0; j < skyIslands.Length; j++)
                {

                    //Get the other Island and Skip it if necessary
                    GameObject otherIsland = skyIslands[j];
                    if(i == j || otherIsland == null) continue ;

                    //Get Data of Other Island
                    Vector3 otherPos = otherIsland.transform.localPosition;
                    float otherRadius = otherIsland.GetComponent<GenerateIsland>().GetRadius();

                    //Calculate Distances
                    Vector3 offset = currPos - otherPos;
                    float distance = offset.magnitude;
                    float minDistance = currRadius + otherRadius;

                    //Calculate repulsion
                    float safeDistance = Mathf.Max(distance, 0.001f);
                    float repulsionValue = repulsionStrength / (safeDistance * safeDistance); //This is called an inverse square falloff
                    totalRepulsion += offset.normalized * repulsionValue;

                }

                //Calculate Boundary repulsion
                //X Axis
                if (currPos.x - currRadius < -halfWidth)
                {
                    totalRepulsion.x += (-halfWidth - (currPos.x - currRadius)) * boundaryRepulsion;
                }
                else if (currPos.x + currRadius > halfWidth)
                {
                    totalRepulsion.x -= ((currPos.x + currRadius) - halfWidth) * boundaryRepulsion;
                }

                //Y Axis
                if (currPos.y - currRadius < -halfHeight)
                {
                    totalRepulsion.y += (-halfHeight - (currPos.y - currRadius)) * boundaryRepulsion;
                }
                else if (currPos.y + currRadius > halfHeight)
                {
                    totalRepulsion.y -= ((currPos.y + currRadius) - halfHeight) * boundaryRepulsion;
                }

                //Z Axis
                if (currPos.z - currRadius < -halfLength)
                {
                    totalRepulsion.z += (-halfLength - (currPos.z - currRadius)) * boundaryRepulsion;
                }
                else if (currPos.z + currRadius > halfLength)
                {
                    totalRepulsion.z -= ((currPos.z + currRadius) - halfLength) * boundaryRepulsion;
                }

                adjustments[i] = totalRepulsion;

            }

            //Apply Adjustments
            for (int i = 0; i < skyIslands.Length; i++)
            {
                if (skyIslands[i] != null)
                    skyIslands[i].transform.localPosition += adjustments[i];
            }

        }

    }

}
