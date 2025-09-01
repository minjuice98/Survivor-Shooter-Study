using UnityEngine;
using UnityEngine.AI;

public class DollAnimation : MonoBehaviour
{
    public float speedZeroThreshold = 0.05f;
    public float speedDampTime = 0.10f;
    public float speedMax = 3.5f;

    Animator animator;
    NavMeshAgent agent;
    LivingEntity living;
    Dolls dolls; // DollData 접근

    readonly int HashSpeed = Animator.StringToHash("Speed");
    readonly int HashIsDead = Animator.StringToHash("IsDead");

    Vector3 prevPos;
    bool useDeltaPosition;
    bool loggedNoController = false;   // 중복 로그 방지

    void Awake()
    {
        animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>(true);
        agent = GetComponent<NavMeshAgent>();
        living = GetComponent<LivingEntity>();
        dolls = GetComponent<Dolls>();
        prevPos = transform.position;

        useDeltaPosition = (agent != null && agent.updatePosition == false);

        TryAssignController(); // 최초 1회 시도
    }

    void OnEnable()
    {
        TryAssignController(); // 재활성화 시에도 재시도
    }

    void TryAssignController()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>(true);
        }

        if (animator != null && animator.runtimeAnimatorController == null && dolls != null && dolls.data != null && dolls.data.controller != null)
        {
            animator.runtimeAnimatorController = dolls.data.controller; // DollData에서 자동 할당
        }

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            if (!loggedNoController)
            {
                loggedNoController = true;
                Debug.LogWarning($"[DollAnimation] Animator Controller missing on '{name}'. Disabling DollAnimation to avoid spam.");
            }
            enabled = false; // 경고 폭주 차단
        }
    }

    void Update()
    {
        // 방어: 런타임에 컨트롤러가 제거된 경우
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            TryAssignController();
            if (!enabled) return; // 여전히 없으면 종료
        }

        bool isDead = (living != null && living.isDead);
        animator.SetBool(HashIsDead, isDead);

        float speed = 0f;
        if (agent != null && !useDeltaPosition) speed = agent.velocity.magnitude;
        else
        {
            Vector3 now = transform.position;
            Vector3 delta = now - prevPos; prevPos = now;
            delta.y = 0f;
            if (Time.deltaTime > 0f) speed = delta.magnitude / Time.deltaTime;
        }

        if (speed < speedZeroThreshold) speed = 0f;
        float normalized = (speedMax > 0.0001f) ? Mathf.Clamp01(speed / speedMax) : speed;
        animator.SetFloat(HashSpeed, normalized, speedDampTime, Time.deltaTime);
    }
}
