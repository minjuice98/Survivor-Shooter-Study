using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; 
    public float rotateSpeed = 180f;

    private PlayerInput input;
    private Rigidbody rb;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        ////player rotate에서만 관리
        //var rotation = Quaternion.Euler(0f, input.rotate * rotateSpeed * Time.deltaTime, 0f);
        //rb.MoveRotation(rb.rotation * rotation);

        //이동
        Vector3 moveDir = new Vector3(input.moveX, 0f, input.moveZ).normalized;
        Vector3 worldMove = transform.TransformDirection(moveDir);

        rb.MovePosition(transform.position + worldMove * moveSpeed * Time.deltaTime);
    }
}