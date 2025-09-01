using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DollContactDamage : MonoBehaviour
{
    [Header("Contact Damage")]
    public float contactDamage = 10f;
    public float damageInterval = 0.2f;  // 0.2초로 변경

    private Dolls doll;
    private Collider triggerCol;
    private bool damaging;
    private Coroutine damageRoutine;

    private void Awake()
    {
        doll = GetComponent<Dolls>();
        triggerCol = GetComponents<Collider>().FirstOrDefault(c => c.isTrigger);
        if (triggerCol == null)
        {
            var sc = gameObject.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.6f;
            triggerCol = sc;
        }
    }

    private void OnEnable()
    {
        if (doll != null && doll.data != null)
            contactDamage = doll.data.damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (doll != null && doll.isDead) return;

        var dmgTarget = other.GetComponentInParent<IDamagable>();
        if (dmgTarget == null) return;

        if (!damaging)
        {
            damaging = true;
            damageRoutine = StartCoroutine(CoDamage(other));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var dmgTarget = other.GetComponentInParent<IDamagable>();
        if (dmgTarget == null) return;

        if (damaging && damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }
        damaging = false;
    }

    private IEnumerator CoDamage(Collider playerCol)
    {
        IDamagable dmgTarget = playerCol?.GetComponentInParent<IDamagable>();
        Transform targetTr = (dmgTarget as Component)?.transform;

        while (damaging && dmgTarget != null && targetTr != null)
        {
            Vector3 hitPos = targetTr.position;
            dmgTarget.OnDamage(contactDamage, hitPos, Vector3.up);

            Debug.Log($"[EnemyContact] Dealing {contactDamage} damage to player");

            yield return new WaitForSeconds(damageInterval);
        }

        damaging = false;
        damageRoutine = null;
    }

    public void EnsureTriggerChild(Transform root)
    {
        if (triggerCol != null && triggerCol.isTrigger) return;

        var child = root.Find("HitZone");
        if (child == null)
        {
            var go = new GameObject("HitZone");
            go.transform.SetParent(root);
            go.transform.localPosition = Vector3.zero;
            child = go.transform;
        }

        var sc = child.GetComponent<SphereCollider>();
        if (sc == null) sc = child.gameObject.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 0.6f;
        triggerCol = sc;
    }
}