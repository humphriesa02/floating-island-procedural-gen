using UnityEngine;

public class IslandObjectCuller : MonoBehaviour
{
    [SerializeField] private float objectRenderDistance = 100f;
    private Transform mainCamera;

    void Start() {
        mainCamera = Camera.main.transform;
    }

    void Update() {
        float dist = Vector3.Distance(mainCamera.position, transform.position);
        bool shouldBeVisible = dist < objectRenderDistance;

        foreach (Transform child in transform)
        {
            if (child.GetComponent<Structure>() != null)
                child.gameObject.SetActive(shouldBeVisible);
        }
    }
}
