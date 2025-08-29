using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public GameObject target;
    public float offsetX;
    public float offsetY;
    public float offsetZ;

    public float delayTime=1f;

    private void Update()
    {
        Vector3 FixedPos = new Vector3(
            target.transform.position.x + offsetX,
            target.transform.position.y + offsetY,
            target.transform.position.z + offsetZ);
        transform.position = Vector3.Lerp(transform.position,FixedPos,Time.deltaTime*delayTime);
    }
}
