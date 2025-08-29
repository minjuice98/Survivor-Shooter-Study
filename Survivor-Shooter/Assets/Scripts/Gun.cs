using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class Gun : MonoBehaviour
{
    private PlayerInput input;

    public Transform BarrelEndPosition;

    public ParticleSystem muzzleFlashEffect;
    public LineRenderer lightlineRenderer;

    private float fireDistance = 50f;
    public float damage = 25f;
    private float lastFireTime;

    private void Awake()
    {
        input = GetComponentInParent<PlayerInput>();
        lightlineRenderer.enabled = false;
        lightlineRenderer.positionCount = 2;
    }

    private void Update()
    {
        if (input.fire1)
        {
            Fire();
        }
    }

    private IEnumerator CoShotEffect(Vector3 hitPosition)
    {
        muzzleFlashEffect.Play();
        lightlineRenderer.enabled = true;
        lightlineRenderer.SetPosition(0, BarrelEndPosition.position);
        Vector3 endPos = hitPosition;
        lightlineRenderer.SetPosition(1, endPos);
        yield return new WaitForSeconds(0.2f);
        lightlineRenderer.enabled = false;
    }

    public void Fire()
    {
        Vector3 hitPosition = Vector3.zero;
        RaycastHit hit;
        if (Physics.Raycast(BarrelEndPosition.position, BarrelEndPosition.forward,
            out hit, fireDistance))
        {
            hitPosition = hit.point;
            var target = hit.collider.GetComponent<IDamagable>();
            if (target != null)
            {
                target.OnDamage(damage, hit.point, hit.normal);
            }
        }
        else
        {
            hitPosition = BarrelEndPosition.position + BarrelEndPosition.forward * fireDistance;
        }
        StartCoroutine(CoShotEffect(hitPosition));
        lastFireTime = Time.time;
    }
}
