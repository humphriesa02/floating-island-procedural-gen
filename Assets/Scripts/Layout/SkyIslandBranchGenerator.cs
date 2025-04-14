using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SkyIslandBranchGenerator : MonoBehaviour
{

    [SerializeField] private List<GameObject> layoutPrefabs;
    [SerializeField] private int maxBranchDepth = 2;
    [SerializeField] private int layoutsPerBranch = 3;
    [SerializeField] private float branchSpacing = 20.0f;
    [SerializeField] private float branchScaleFactor = 0.8f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setLayoutPrefabs(List<GameObject> list)
    {
        layoutPrefabs = list;
    }

    public void GenerateBranch(Vector3 startPosition, Quaternion startRotation, int depth = 0)
    {

        //Dont Generate a branch if the max depth is exceeded
        if (depth > maxBranchDepth) return;

        //Initialize Loop Variables
        SkyIslandLayout prevLayout = null;
        Vector3 currentPos = startPosition;
        Quaternion currentRot = startRotation;

        //Generate each of the Layouts for the Current Branch
        for (int i = 0; i < layoutsPerBranch; i++)
        {

            //Create the Layout at Random
            GameObject prefab = layoutPrefabs[Random.Range(0, layoutPrefabs.Count)];
            GameObject layoutGO = Instantiate(prefab, currentPos, currentRot, transform);

            //Get the Layout
            SkyIslandLayout layout = layoutGO.GetComponent<SkyIslandLayout>();
            if(layout == null) continue;

            //Update the Links
            if(prevLayout != null)
            {
                layout.SetPreviousLayout(prevLayout);
                prevLayout.SetNextLayout(layout);
            }

            //Generat the Islands
            layout.GenerateIslands();

            //Update the Loop Variables
            currentPos = layout.getExitPoint().transform.position;
            prevLayout = layout;

            //Generater Sub Branch
            if (Random.value < 0.25f && depth < maxBranchDepth)
            {
                Vector3 branchStart = layout.getLeftPoint().transform.position;
                Quaternion branchRot = layout.getLeftPoint().transform.rotation;

                GenerateBranch(branchStart, branchRot, depth + 1);
            }

        }

    }

}
