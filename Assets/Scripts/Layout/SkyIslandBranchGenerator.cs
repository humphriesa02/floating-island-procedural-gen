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

            //Allign the Layout
            if (prevLayout == null)
            {
                AlignLayouts(layout, startPosition);
            }
            else
            {
                AlignLayouts(layout, prevLayout.getExitPoint().transform.position);
            }

            //Update the Links
            if(prevLayout != null)
            {
                layout.SetPreviousLayout(prevLayout);
                prevLayout.SetNextLayout(layout);
            }

            //Generat the Islands
            layout.GenerateIslands();
            layout.GradiantSeparation();

            //Update the Loop Variables
            currentPos = layout.getExitPoint().transform.position;
            prevLayout = layout;

            //Generater Sub Branch
            if (Random.value < 0.15f && depth == 0)
            {
                if (Random.value > 0.5f)
                {
                    Vector3 branchStart = layout.getLeftPoint().transform.position;
                    Vector3 forward = layout.getLeftPoint().transform.position - layout.transform.position;
                    Quaternion branchRot = Quaternion.LookRotation(forward, Vector3.up);
                    GenerateBranch(branchStart, branchRot, depth + 1);
                }
                else
                {
                    Vector3 branchStart = layout.getRightPoint().transform.position;
                    Vector3 forward = layout.getRightPoint().transform.position - layout.transform.position;
                    Quaternion branchRot = Quaternion.LookRotation(forward, Vector3.down);
                    GenerateBranch(branchStart, branchRot, depth + 1);
                }

            }

        }

    }

    private void AlignLayouts(SkyIslandLayout layout, Vector3 startPoint)
    {

        //Get Transform
        Transform entry = layout.getEntryPoint().transform;

        Vector3 offset = entry.position - layout.transform.position;
        layout.transform.position = startPoint - offset;

    }

}
