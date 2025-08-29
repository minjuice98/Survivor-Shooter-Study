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
        var rotation = Quaternion.Euler(0f, input.rotate * rotateSpeed * Time.deltaTime, 0f);
        rb.MoveRotation(rb.rotation * rotation);

        //¿Ãµø
        var distance = input.move * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + distance * transform.forward);
    }
}