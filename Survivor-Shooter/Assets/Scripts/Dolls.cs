using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Dolls : LivingEntity
{
    [Header("Data")]
    public DollData data;

    [Header("Runtime")]
    public Transform target;

    private NavMeshAgent agent;
    private CapsuleCollider capsule;
    private Rigidbody rb;

    // === 회전 튜닝 ===
    [Header("Facing")]
    [Tooltip("초당 회전 속도(도/초)")]
    public float turnSpeedDegPerSec = 720f;
    [Tooltip("이 거리 이내면 steeringTarget 대신 플레이어를 직접 본다")]
    public float facePlayerDistance = 1.2f;

    // === Carve만으로 붙는 거리 제어 ===
    [Header("Carve-Aware Control")]
    [Tooltip("멈출 때 보이는 아주 작은 시각적 여유 (0.01~0.03)")]
    public float visualGap = 0.02f;
    [Tooltip("Obstacle 가장자리 근처에 들어갈 때 판정 여유(들어갈 때)")]
    public float enterSlack = 0.02f;
    [Tooltip("Obstacle 가장자리에서 벗어날 때 판정 여유(나올 때, enter보다 크게)")]
    public float exitSlack = 0.05f;
    [Tooltip("멈추기 전 서서히 감속이 시작되는 폭(m)")]
    public float slowBand = 0.35f;

    private bool inCloseRange = false;
    private ObstacleAvoidanceType originalAvoidance;
    private CapsuleCollider playerCapsule;
    private NavMeshObstacle playerObstacle;
    private float baseSpeed;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (capsule == null) capsule = GetComponent<CapsuleCollider>();
        if (rb == null) rb = GetComponent<Rigidbody>();

        // 키네마틱 RB + 네비가 직접 트랜스폼을 안 건드리게
        rb.isKinematic = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.angularSpeed = 0f; // 내부 회전 끔

        if (data != null)
        {
            maxHealth = data.maxHp;
            health = maxHealth;

            agent.speed = data.speed;
            baseSpeed = data.speed;

            // 콜라이더와 정합
            agent.radius = Mathf.Clamp(capsule.radius * 0.95f, 0.15f, 1.0f);
            agent.height = Mathf.Max(agent.height, capsule.height);

            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = Random.Range(30, 70);
        }
        else
        {
            baseSpeed = agent.speed;
        }

        originalAvoidance = agent.obstacleAvoidanceType;

        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
        if (target != null)
        {
            playerCapsule = target.GetComponent<CapsuleCollider>();
            playerObstacle = target.GetComponent<NavMeshObstacle>();
        }

        // 초기 동기화
        agent.nextPosition = transform.position;
        agent.isStopped = false;
        // stoppingDistance는 시각적 갭만 사용 (Carve가 실제 간격 결정)
        agent.stoppingDistance = Mathf.Max(0.001f, visualGap);
    }

    private void Update()
    {
        if (isDead || agent == null || target == null) return;

        // 프레임 시작: 위치 동기화 (지터 방지 핵심)
        agent.nextPosition = rb.position;

        // Obstacle 가장자리까지의 수평 여유 계산
        float d = HorizontalDistanceToPlayer();
        float obsR = EstimateObstacleRadius();        // 0(없으면)
        float toEdge = d - (obsR + agent.radius);     // +면 가장자리 밖, 0이면 딱 경계

        bool carveActive = (playerObstacle != null && playerObstacle.carving);

        // Carve가 켜져 있으면 stoppingDistance는 아주 작게만 둠(시각용)
        agent.stoppingDistance = Mathf.Max(0.001f, visualGap);

        // 히스테리시스: 가장자리 근처에서만 네비 정지해 떨림 차단
        if (!inCloseRange && carveActive && toEdge <= enterSlack) inCloseRange = true;
        else if (inCloseRange && (!carveActive || toEdge >= exitSlack)) inCloseRange = false;

        if (inCloseRange)
        {
            // 근접: 네비 정지 + 회피 OFF (줄다리기 차단)
            if (!agent.isStopped)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

            // 이동은 중단, 회전만 유지
            FaceTowardsTarget();
            agent.nextPosition = rb.position; // 동기화
        }
        else
        {
            // 추격 모드
            if (agent.isStopped) agent.isStopped = false;
            agent.obstacleAvoidanceType = originalAvoidance;

            agent.SetDestination(target.position);

            // 멈추기 전에 감속 (가장자리로 다가갈수록 느려짐)
            float t = Mathf.InverseLerp(0f, slowBand, Mathf.Max(0f, toEdge));
            agent.speed = Mathf.Lerp(baseSpeed * 0.4f, baseSpeed, t);

            // desiredVelocity 기반으로 1프레임 이동
            Vector3 vel = agent.desiredVelocity; vel.y = 0f;
            Vector3 step = (vel.sqrMagnitude > 0.0001f)
                ? vel.normalized * (agent.speed * Time.deltaTime)
                : Vector3.zero;

            // ── 경계 클램프: 이번 프레임 step이 Carve 가장자리를 넘지 않게 제한 (큰 코끼리용 안정화)
            Vector3 playerXZ = new Vector3(target.position.x, rb.position.y, target.position.z);
            Vector3 toPlayer = playerXZ - rb.position;
            float dist = toPlayer.magnitude;

            // “가장자리”까지의 목표 거리 = Obstacle반경 + agent.radius + visualGap
            float edge = obsR + agent.radius + Mathf.Max(0.001f, visualGap);

            if (step.sqrMagnitude > 0.000001f && dist > 0.0001f)
            {
                Vector3 forwardToPlayer = toPlayer / dist;              // 플레이어 방향 단위벡터
                float proj = Vector3.Dot(step, forwardToPlayer);        // step이 플레이어 방향으로 얼마나 나아가나

                // 이번 프레임 뒤에 (dist - proj) 가 edge보다 작아지면, 넘치는 만큼 줄인다
                if (proj > 0f && (dist - proj) < edge)
                {
                    float allowForward = Mathf.Max(0f, dist - edge);    // 이번 프레임 허용되는 전진량
                    Vector3 stepForwardClamped = forwardToPlayer * Mathf.Min(allowForward, proj);
                    Vector3 stepSide = step - (forwardToPlayer * proj); // 측면 성분은 그대로
                    step = stepSide + stepForwardClamped;
                }
            }

            rb.MovePosition(rb.position + step);
            agent.nextPosition = rb.position;

            FaceTowardsTarget();
        }
    }

    private float HorizontalDistanceToPlayer()
    {
        Vector3 a = rb.position; a.y = 0f;
        Vector3 b = target.position; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private float EstimateObstacleRadius()
    {
        if (playerObstacle == null || !playerObstacle.carving) return 0f;

        if (playerObstacle.shape == NavMeshObstacleShape.Capsule)
            return playerObstacle.radius;

        // Box인 경우 평면 최대 반쪽 길이 사용
        return 0.5f * Mathf.Max(playerObstacle.size.x, playerObstacle.size.z);
    }

    private void FaceTowardsTarget()
    {
        // 기본은 steeringTarget, 가까우면 플레이어 직접
        Vector3 lookDir = agent.steeringTarget - rb.position;
        Vector3 toPlayer = target.position - rb.position;
        if (toPlayer.sqrMagnitude <= facePlayerDistance * facePlayerDistance)
            lookDir = toPlayer;

        lookDir.y = 0f;
        if (lookDir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        Quaternion newRot = Quaternion.RotateTowards(
            rb.rotation,
            targetRot,
            turnSpeedDegPerSec * Time.deltaTime
        );
        rb.MoveRotation(newRot);
    }

    protected override void Die()
    {
        if (isDead) return;

        base.Die();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        Destroy(gameObject);
    }
}
