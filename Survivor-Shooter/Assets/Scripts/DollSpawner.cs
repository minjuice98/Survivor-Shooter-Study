using UnityEngine;
using UnityEngine.AI;

public class DollSpawner : MonoBehaviour
{
    public RuntimeAnimatorController defaultController;

    [System.Serializable]
    public class SpawnEntry
    {
        public string name;
        public DollData data;                // 프리팹/스탯/컨트롤러 포함 (data.controller 사용)
        public Transform spawnPoint;         // 고정 스폰 위치 (필수)
        public float interval = 3f;          // 토끼/곰 = 3, 코끼리 = 30
        [Tooltip("Inspector에서 켜두면 시작 즉시 1마리 스폰 후 타이머 진행")]
        public bool spawnOnStart = true;

        [HideInInspector] public float timer;
    }

    [Header("Spawn List (3 types)")]
    public SpawnEntry rabbit;   // interval = 3
    public SpawnEntry bear;     // interval = 3
    public SpawnEntry elephant; // interval = 30

    [Header("Optional")]
    public Transform container; // 생성물을 정리할 부모

    private void Start()
    {
        InitEntry(rabbit);
        InitEntry(bear);
        InitEntry(elephant);
    }

    private void Update()
    {
        TickSpawn(rabbit);
        TickSpawn(bear);
        TickSpawn(elephant);
    }

    private void InitEntry(SpawnEntry e)
    {
        if (e == null) return;
        e.timer = e.spawnOnStart ? e.interval : 0f;

        // 시작 즉시 스폰 옵션
        if (e.spawnOnStart)
        {
            Spawn(e);
            e.timer = 0f;
        }
    }

    private void TickSpawn(SpawnEntry e)
    {
        if (e == null || e.data == null || e.data.dollPrefab == null || e.spawnPoint == null) return;

        e.timer += Time.deltaTime;
        if (e.timer >= e.interval)
        {
            Spawn(e);
            e.timer = 0f;
        }
    }

    // === 스폰 보장 로직 ===
    // === 스폰 보장 로직 ===
    private void Spawn(SpawnEntry e)
    {
        GameObject go = Instantiate(
            e.data.dollPrefab,
            e.spawnPoint.position,
            e.spawnPoint.rotation,
            container
        );
        go.name = $"{e.name}_Clone";

        // Dolls 보장 + 데이터 주입
        var enemy = go.GetComponent<Dolls>();
        if (enemy == null) enemy = go.AddComponent<Dolls>();
        enemy.data = e.data;

        // === Animator 찾기: 자식까지 우선 ===
        Animator anim = go.GetComponentInChildren<Animator>(true);
        if (anim == null)
        {
            // 혹시 legacy Animation만 달려 있나?
            var legacy = go.GetComponentInChildren<Animation>(true);
            if (legacy != null)
            {
                Debug.LogWarning($"[SPAWN] '{go.name}' has legacy Animation component, not Animator. " +
                                 $"Convert to Mecanim (Animator) or place Animator on the model.");
            }
            // 최후: 루트에 Animator 추가 (가급적 모델 자식에 직접 넣는 걸 추천)
            anim = go.AddComponent<Animator>();
        }

        // === 컨트롤러 할당 (우선순위: DollData.controller → Spawner.defaultController → 프리팹에 이미 있는 값) ===
        if (anim.runtimeAnimatorController == null)
        {
            var ctrl = (e.data != null && e.data.controller != null) ? e.data.controller : defaultController;
            if (ctrl != null) anim.runtimeAnimatorController = ctrl;
        }

        // NavMeshAgent 보장
        var agent = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent == null) agent = go.AddComponent<UnityEngine.AI.NavMeshAgent>();

        // 본체 콜라이더 보장(없으면 Capsule 추가) — 밀림 방지하려면 트리거 운용 권장
        var col = go.GetComponent<Collider>();
        if (col == null)
        {
            var cap = go.AddComponent<CapsuleCollider>();
            cap.isTrigger = true;
            col = cap;
        }

        // 접촉용 트리거 콜라이더 최소 1개 보장
        bool hasTrigger = false;
        var cols = go.GetComponents<Collider>();
        foreach (var c in cols) { if (c != null && c.isTrigger) { hasTrigger = true; break; } }
        if (!hasTrigger)
        {
            var sc = go.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.6f;
            hasTrigger = true;
        }

        // 접촉 데미지 보장
        var contact = go.GetComponent<DollContactDamage>();
        if (contact == null) contact = go.AddComponent<DollContactDamage>();

        // 애니메이션 드라이버 보장 (자식 Animator도 자동 찾도록 구현돼 있어야 함)
        var ea = go.GetComponent<DollAnimation>();
        if (ea == null) ea = go.AddComponent<DollAnimation>();

        // 컨트롤러 최종 체크: 없으면 애니 드라이버 비활성화(스팸 방지)
        if (anim.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"[SPAWN] '{go.name}' has NO AnimatorController even after fallback. " +
                             $"Assign DollData.controller or Spawner.defaultController, or set controller on prefab.");
            if (ea != null) ea.enabled = false;
        }

        string ctrlName = anim.runtimeAnimatorController ? anim.runtimeAnimatorController.name : "NULL";
        Debug.Log($"[SPAWN] {go.name} | animator={(anim != null ? anim.transform.name : "NULL")} | ctrl={ctrlName} | agentSpeed={(agent ? agent.speed : 0)} | hasTrigger={hasTrigger}");
    }

}
