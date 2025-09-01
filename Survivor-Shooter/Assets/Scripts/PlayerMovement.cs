using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 180f;

    [Header("Idle State")]
    public bool isIdle = false;  // Idle 상태 플래그
    public float idleThreshold = 0.1f; // 입력이 이 값보다 작으면 Idle

    private PlayerInput input;
    private Rigidbody rb;
    private Vector3 lastValidPosition; // Idle 시 고정할 위치

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        lastValidPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // 이동 입력만 체크 (회전은 제외)
        float moveInput = Mathf.Abs(input.move);

        // Idle 상태 결정 - 이동 입력이 없을 때만
        if (moveInput < idleThreshold)
        {
            if (!isIdle)
            {
                // Idle 진입 시 현재 위치 저장
                isIdle = true;
                lastValidPosition = transform.position;
                rb.linearVelocity = Vector3.zero;
            }

            // Idle 상태에서는 위치 강제 고정, 하지만 회전은 허용
            rb.MovePosition(lastValidPosition);

            // 회전은 Idle 상태에서도 동작
            if (Mathf.Abs(input.rotate) > 0.01f)
            {
                rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, input.rotate * rotateSpeed * Time.deltaTime, 0f));
            }
            return;
        }
        else
        {
            if (isIdle)
            {
                // Idle에서 벗어날 때
                isIdle = false;
                lastValidPosition = transform.position;
            }
        }

        // 일반 이동 로직 (이동 입력이 있을 때)
        var rotation = Quaternion.Euler(0f, input.rotate * rotateSpeed * Time.deltaTime, 0f);
        rb.MoveRotation(rb.rotation * rotation);

        var distance = input.move * moveSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + distance * transform.forward;
        rb.MovePosition(newPosition);
        lastValidPosition = newPosition;
    }
}