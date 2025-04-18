using System.Collections.Generic;
using UnityEngine;

public class SkyIslandSpiralGenerator : MonoBehaviour
{

    [Header("Order of Layout Repetition")]
    [SerializeField] private List<GameObject> layoutPrefabs;

    [Header("Spiral Settings")]
    [SerializeField] private int numRepetitions = 10;
    [SerializeField] private float angleStepCoilDegrees = 45f;
    [SerializeField] private float angleEvelationAngle = 22.5f;

    private SkyIslandLayout prevLayout;
    private float accumulatedCoilAngle = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateSpiral();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateSpiral()
    {

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

                //Update previous layout
                prevLayout = layout;

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
