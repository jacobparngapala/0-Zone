using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    private float fixedZ;

    void Start()
    {
        fixedZ = transform.position.z;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (target == null)
            target = player.transform;
        else
            Debug.LogWarning("NO GAME OBJECT!");
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, fixedZ);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
