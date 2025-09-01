using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Flash : MonoBehaviour
{
    [Header("Flash")]
    [Tooltip("플래시가 최대일 때의 알파값(0~1)")]
    public float maxAlpha = 0.35f;
    [Tooltip("올라가는 시간(초)")]
    public float fadeIn = 0.06f;
    [Tooltip("사라지는 시간(초)")]
    public float fadeOut = 0.25f;
    [Tooltip("데미지 크기에 비례해서 강도 스케일")]
    public bool scaleByDamage = true;

    private Image img;
    private Coroutine co;

    void Awake()
    {
        img = GetComponent<Image>();
        img.raycastTarget = false;           // UI 입력 막지 않도록
        Color c = img.color; c.a = 0f; img.color = c;
    }

    /// <summary>
    /// 화면 플래시. damage/max 를 0~1로 넣으면 강도 자동 스케일.
    /// </summary>
    public void Pulse(float damage01 = 1f)
    {
        if (!gameObject.activeInHierarchy) return;
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(CoPulse(Mathf.Clamp01(damage01)));
    }

    IEnumerator CoPulse(float dmg01)
    {
        float target = scaleByDamage ? Mathf.Lerp(0.15f, maxAlpha, dmg01) : maxAlpha;

        // up
        float t = 0f;
        Color c = img.color;
        float startA = c.a;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(startA, target, t / Mathf.Max(0.0001f, fadeIn));
            img.color = c;
            yield return null;
        }
        // down
        t = 0f; startA = img.color.a;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(startA, 0f, t / Mathf.Max(0.0001f, fadeOut));
            img.color = c;
            yield return null;
        }
        c.a = 0f; img.color = c;
        co = null;
    }
}
