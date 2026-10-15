using UnityEngine;

public class ShootingScript : MonoBehaviour
{
    public Transform shotPoint;
    public GameObject bullet;
    public float bulletSpeed;

    void Update()
    {
        AimAtCursor();

        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void AimAtCursor()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = shotPoint.position.z;

        Vector2 direction = (Vector2)mouseWorldPos - (Vector2)shotPoint.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // -90 because the fire direction is local "up" (+Y), not local "right" (+X)
        shotPoint.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    void Shoot()
    {
        GameObject bulletInstance = Instantiate(bullet, shotPoint.position, shotPoint.rotation);
        Rigidbody2D rb = bulletInstance.GetComponent<Rigidbody2D>();
        rb.AddForce(shotPoint.up * bulletSpeed, ForceMode2D.Impulse);

        Collider2D bulletCollider = bulletInstance.GetComponent<Collider2D>();
        Collider2D shooterCollider = GetComponent<Collider2D>();
        if (bulletCollider != null && shooterCollider != null)
        {
            Physics2D.IgnoreCollision(bulletCollider, shooterCollider);
        }
    }
}