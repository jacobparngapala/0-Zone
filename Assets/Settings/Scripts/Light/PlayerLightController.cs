using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(InventoryManager))]
public class PlayerLightController : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private Light2D spotlight;   // the Flashlight object
    [SerializeField] private Light2D glow;        // the Glow object (stays on)

    [Header("Shot Flash")]
    [SerializeField] private float flashIntensity = 3f;   // peak brightness of the spotlight flash
    [SerializeField] private float holdTime = 0.05f;      // stays at peak this long
    [SerializeField] private float fadeTime = 0.25f;      // then fades out over this long
    [SerializeField] private float glowPulse = 1.8f;      // glow brightens by this multiple

    private InventoryManager inventory;
    private float spotBaseIntensity = 1f;
    private float glowBaseIntensity = 1f;
    private Coroutine flashRoutine;

    private void Awake()
    {
        inventory = GetComponent<InventoryManager>();
        if (spotlight != null) spotBaseIntensity = spotlight.intensity;
        if (glow != null) glowBaseIntensity = glow.intensity;
    }

    private void OnEnable() { inventory.OnSelectionChanged += OnSelectionChanged; }
    private void OnDisable() { inventory.OnSelectionChanged -= OnSelectionChanged; }

    private void OnSelectionChanged(int index, ItemData item)
    {
        StopFlash();
        bool flashlightInHand = item != null && item.type == ItemType.Flashlight;
        if (spotlight != null) spotlight.enabled = flashlightInHand;
    }

    // Called by GunController when a shot is fired
    public void Flash(float zAngle)
    {
        if (spotlight == null) return;
        spotlight.transform.rotation = Quaternion.Euler(0f, 0f, zAngle);

        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        spotlight.enabled = true;
        spotlight.intensity = flashIntensity;
        if (glow != null) glow.intensity = glowBaseIntensity * glowPulse;

        yield return new WaitForSeconds(holdTime);

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeTime);
            spotlight.intensity = Mathf.Lerp(flashIntensity, 0f, k);
            if (glow != null)
                glow.intensity = Mathf.Lerp(glowBaseIntensity * glowPulse, glowBaseIntensity, k);
            yield return null;
        }

        RestoreIntensities();
        spotlight.enabled = false;   // back to "no spotlight" while holding the gun
        flashRoutine = null;
    }

    private void StopFlash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        RestoreIntensities();
    }

    private void RestoreIntensities()
    {
        if (spotlight != null) spotlight.intensity = spotBaseIntensity;
        if (glow != null) glow.intensity = glowBaseIntensity;
    }
}