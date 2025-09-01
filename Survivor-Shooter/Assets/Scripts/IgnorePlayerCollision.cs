using UnityEngine;
using System.Collections;

[DefaultExecutionOrder(100)]
public class IgnorePlayerCollision : MonoBehaviour
{
    private static Collider[] playerCols;
    private Collider[] myCols;
    private static bool isPlayerColsCached = false;

    void Awake()
    {
        myCols = GetComponentsInChildren<Collider>(includeInactive: true);
        StartCoroutine(DelayedSetup());
    }

    private IEnumerator DelayedSetup()
    {
        // 몇 프레임 기다린 후 설정 (다른 오브젝트들이 초기화될 시간)
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        CachePlayerCols();
        ApplyIgnore();

        // 1초 후에 한 번 더 확인 (안전장치)
        yield return new WaitForSeconds(1f);
        ApplyIgnore();
    }

    void OnEnable()
    {
        StartCoroutine(DelayedSetup());
    }

    private void CachePlayerCols()
    {
        if (isPlayerColsCached && playerCols != null) return;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            playerCols = p.GetComponentsInChildren<Collider>(includeInactive: true);
            isPlayerColsCached = true;
            Debug.Log($"[IgnoreCollision] Cached {playerCols.Length} player colliders");
        }
    }

    private void ApplyIgnore()
    {
        CachePlayerCols();
        if (playerCols == null || myCols == null) return;

        int ignoredCount = 0;

        foreach (var mine in myCols)
        {
            if (mine == null || !mine.enabled) continue;
            if (mine.isTrigger) continue; // 트리거는 데미지용으로 유지

            foreach (var pc in playerCols)
            {
                if (pc == null || !pc.enabled) continue;

                // 이미 무시되고 있는지 확인하지 않고 무조건 설정
                Physics.IgnoreCollision(mine, pc, true);
                ignoredCount++;
            }
        }

        Debug.Log($"[IgnoreCollision] {gameObject.name}: Set {ignoredCount} collision ignores");
    }
}
