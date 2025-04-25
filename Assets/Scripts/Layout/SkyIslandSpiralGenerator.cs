using System.Collections.Generic;
using UnityEngine;

public class SkyIslandSpiralGenerator : MonoBehaviour
{

    [Header("Order of Layout Repetition")]
    [SerializeField] private List<GameObject> layoutPrefabs;

    [Header("Spiral Settings")]
    [SerializeField] private int numRepetitions = 10;
    [SerializeField] private float angleStepCoilDegreesMAX = 50f;
    [SerializeField] private float angleStepCoilDegreesMIN = 40f;
    [SerializeField] private float angleEvelationAngleMAX = 30f;
    [SerializeField] private float angleEvelationAngleMIN = 15f;

    private float angleStepCoilDegrees = 0.0f;
    private float angleEvelationAngle = 0.0f;

    private SkyIslandLayout prevLayout;
    private float accumulatedCoilAngle = 0.0f;
    
    private SkyIslandBranchGenerator branchGenerator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Determine Randomness in Range
        angleStepCoilDegrees = Random.Range(angleStepCoilDegreesMIN, angleStepCoilDegreesMAX);
        angleEvelationAngle = Random.Range(angleEvelationAngleMIN, angleEvelationAngleMAX);

        branchGenerator = gameObject.GetComponent<SkyIslandBranchGenerator>();
        if (branchGenerator != null)
        {
            branchGenerator.setLayoutPrefabs(layoutPrefabs);
        }
        GenerateSpiral();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateSpiral()
    {

        int layoutsSinceLastBranch = 0;
        int nextBranchThreshold = Random.Range(10, 21);

        for (int i = 0; i < numRepetitions; i++)
        {

            foreach (GameObject prefab in layoutPrefabs)
            {
                //Create an instance of the prefab
                GameObject currentGO = Instantiate(prefab, transform);

                //Confirm it has a layout component
                SkyIslandLayout layout = currentGO.GetComponent<SkyIslandLayout>();
                if (layout == null)
                {
                    Debug.LogWarning(prefab.name + " does not contain SkyIslandLayout");
                    continue;
                }

                //If its the first layout, just place it
                if (prevLayout == null)
                {
                    layout.transform.position = transform.position;
                    layout.transform.rotation = Quaternion.identity;
                }
                //Otherwise, base its position on that of the previous layout
                else
                {
                    //Spiral Up
                    layout.transform.position = prevLayout.getExitPoint().transform.position;

                    //Allign Entry and Exit
                    AlignLayouts(layout, prevLayout);

                    //Spiral Around
                    layout.transform.RotateAround(prevLayout.getExitPoint().transform.position, Vector3.up, accumulatedCoilAngle);
                    layout.transform.RotateAround(prevLayout.getExitPoint().transform.position, prevLayout.getExitPoint().transform.right, -angleEvelationAngle);
                    accumulatedCoilAngle += angleStepCoilDegrees;

                    //Link the layouts
                    layout.SetPreviousLayout(prevLayout);
                    prevLayout.SetNextLayout(layout);
                }

                //Generate Islands
                layout.GenerateIslands();
                layout.GradiantSeparation();

                //Update previous layout
                prevLayout = layout;

                //Possibly Create Branch
                layoutsSinceLastBranch++;
                if (layoutsSinceLastBranch >= nextBranchThreshold)
                {
                    if (branchGenerator != null)
                    {
                        Vector3 branchStartPoint = layout.getLeftPoint().transform.position;
                        Vector3 forward = layout.getLeftPoint().transform.position - layout.transform.position;
                        Quaternion branchRotation = Quaternion.LookRotation(forward, Vector3.up);

                        branchGenerator.GenerateBranch(branchStartPoint, branchRotation);
                    }

                    layoutsSinceLastBranch = 0;
                    nextBranchThreshold = Random.Range(10, 20);
                }

            }

        }

    }

    private void AlignLayouts(SkyIslandLayout layout, SkyIslandLayout prevLayout)
    {

        //Get Transform
        Transform entry = layout.getEntryPoint().transform;
        Transform exit = prevLayout.getExitPoint().transform;

        Vector3 offset = entry.position - layout.transform.position;
        layout.transform.position = exit.position - offset;

    }

}
