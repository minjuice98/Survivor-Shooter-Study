using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    public UnityEngine.Camera cam; //main camera
    public float rotationSpeed = 5f;

    private void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);

        float rayLength;

        if (ground.Raycast(ray, out rayLength))
        {
            Vector3 lookDistance = ray.GetPoint(rayLength);
            transform.LookAt(new Vector3(lookDistance.x, transform.position.y, lookDistance.z));
        }
    }
}
