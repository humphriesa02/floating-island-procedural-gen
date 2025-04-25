using UnityEngine;

public class FlyingController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSensitivity = 2f;

    private float yaw = 0f;
    private float pitch = 0f;

    void Update()
    {
        // Mouse Look
        yaw += lookSensitivity * Input.GetAxis("Mouse X");
        pitch -= lookSensitivity * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -89f, 89f); // prevent full flip

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);

        // Movement
        Vector3 direction = new Vector3(
            Input.GetAxis("Horizontal"),
            Input.GetKey(KeyCode.Space) ? 1f : Input.GetKey(KeyCode.LeftControl) ? -1f : 0f,
            Input.GetAxis("Vertical")
        );

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            moveSpeed *= 2; // double speed on shift
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            moveSpeed /= 2; // reset speed on shift release
        }

        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.Self);
    }
}
