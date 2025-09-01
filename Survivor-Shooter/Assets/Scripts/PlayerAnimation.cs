using UnityEngine;

/// <summary>
/// Idle/Move/Death 3개 클립 전환용 애니메이션 드라이버.
/// - Speed: 현재 이동 속도(수평)를 댐핑해서 Animator에 전달
/// - IsDead: true가 되면 Death로 크로스페이드하고, 이동/회전 스크립트 끔
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    [Tooltip("죽으면 비활성화할 이동 스크립트(옵션)")]
    public MonoBehaviour[] movementScriptsToDisable; // 예: PlayerMovement, PlayerRotate 등

    [Header("Speed Sampling")]
    [Tooltip("속도→애니메이터 전달 시 댐핑 시간(초)")]
    public float speedDampTime = 0.1f;
    [Tooltip("속도가 이 값 미만이면 강제로 0 처리(소음 제거)")]
    public float speedZeroThreshold = 0.02f;
    [Tooltip("속도 최대값(정규화용). 대략 달리기 속도에 맞춰 조정")]
    public float speedMax = 5.0f;

    [Header("Death")]
    [Tooltip("Death 재생 시 크로스페이드 시간(초)")]
    public float deathCrossfade = 0.1f;
    [Tooltip("Death 상태 이름(Animator)")]
    public string deathStateName = "Death";

    // Animator parameter keys
    static readonly int HashSpeed = Animator.StringToHash("Speed");
    static readonly int HashIsDead = Animator.StringToHash("IsDead");

    // 내부 상태
    private Vector3 _prevPos;
    private bool _deadPlayed = false;

    // 외부에서 참조할 수 있는 사망 상태(프로젝트에 따라 LivingEntity 등과 연결)
    // 여기서는 단순히 public set 가능 플래그 제공. 게임 로직에서 true로 바꿔도 되고,
    // LivingEntity의 isDead를 GetComponent로 읽어와도 됨.
    [HideInInspector] public bool isDead = false;

    void Reset()
    {
        animator = GetComponent<Animator>();
    }

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        _prevPos = transform.position;

        // 안전: 루트모션은 기본 꺼 두기(물리/스크립트 이동 기준)
        animator.applyRootMotion = false;
    }

    void Update()
    {
        // 1) Death 처리: 최초 진입 시 한 번만 재생 세팅
        if (isDead)
        {
            if (!_deadPlayed)
            {
                _deadPlayed = true;

                // Animator 파라미터 세팅 + 크로스페이드
                animator.SetBool(HashIsDead, true);
                if (!string.IsNullOrEmpty(deathStateName))
                {
                    animator.CrossFadeInFixedTime(deathStateName, deathCrossfade);
                }

                // 이동/회전 스크립트 비활성화(옵션)
                if (movementScriptsToDisable != null)
                {
                    foreach (var mb in movementScriptsToDisable)
                    {
                        if (mb != null) mb.enabled = false;
                    }
                }
            }

            // 사망 중에는 Speed를 0으로 고정
            animator.SetFloat(HashSpeed, 0f);
            _prevPos = transform.position;
            return;
        }

        // 2) 속도 샘플링(수평): 실제 이동량 기반 → Idle/Move 전환 안정
        Vector3 now = transform.position;
        Vector3 delta = now - _prevPos;
        _prevPos = now;

        // 수직 성분 제거(지형 경사 영향 배제)
        delta.y = 0f;
        float speed = (Time.deltaTime > 0f) ? (delta.magnitude / Time.deltaTime) : 0f;

        // 작은 떨림 제거
        if (speed < speedZeroThreshold) speed = 0f;

        // 정규화(선택): 애니메이션 블렌딩이 0~1 기준이면 나눠서 넣기
        float normalized = (speedMax > 0.0001f) ? Mathf.Clamp01(speed / speedMax) : speed;

        // 3) Animator 전달(댐핑)
        animator.SetFloat(HashSpeed, normalized, speedDampTime, Time.deltaTime);
    }

    /// <summary>
    /// 외부에서 사망을 통지하는 헬퍼(예: LivingEntity.Die()에서 호출)
    /// </summary>
    public void PlayDeath()
    {
        isDead = true;
    }

    /// <summary>
    /// 루트모션을 쓰고 싶을 때(애니메이션이 이동을 구동할 때) 호출해서 켤 수 있음.
    /// 기본은 false(스크립트 이동 기준).
    /// </summary>
    public void SetApplyRootMotion(bool enabled)
    {
        if (animator) animator.applyRootMotion = enabled;
    }
}
