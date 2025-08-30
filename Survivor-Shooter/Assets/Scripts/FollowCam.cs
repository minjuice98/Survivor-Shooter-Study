using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public GameObject target;
    public float offsetX;
    public float offsetY;
    public float offsetZ;

    public float followSpeed = 10f;

    private void Update()
    {
        Vector3 targetPos = new Vector3(
            target.transform.position.x + offsetX,
            target.transform.position.y + offsetY,
            target.transform.position.z + offsetZ);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
    }
}
