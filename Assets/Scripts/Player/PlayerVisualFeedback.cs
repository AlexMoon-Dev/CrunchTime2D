using System.Collections;
using UnityEngine;

/// <summary>
/// Debug visual feedback: red tint on hit, green outline border on attack.
/// Remove or disable once real animations are in place.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisualFeedback : MonoBehaviour
{
    [Header("Hit Flash")]
    public float hitFlashDuration = 0.2f;

    [Header("Attack Outline")]
    public float  attackOutlineDuration = 0.25f;
    public float  outlineScale          = 1.15f;   // how much bigger the outline is

    private SpriteRenderer _sr;
    private SpriteRenderer _outlineSr;
    private Color          _baseColor;

    private Coroutine _hitRoutine;
    private Coroutine _attackRoutine;

    private void Awake()
    {
        _sr        = GetComponent<SpriteRenderer>();
        _baseColor = _sr.color;

        BuildOutline();
    }

    private void BuildOutline()
    {
        var outlineGo = new GameObject("_AttackOutline");
        outlineGo.transform.SetParent(transform, false);
        outlineGo.transform.localPosition = Vector3.zero;
        outlineGo.transform.localScale    = new Vector3(outlineScale, outlineScale, 1f);

        _outlineSr             = outlineGo.AddComponent<SpriteRenderer>();
        _outlineSr.sprite      = _sr.sprite;
        _outlineSr.color       = new Color(0f, 1f, 0f, 0.8f);
        _outlineSr.sortingOrder = _sr.sortingOrder - 1;   // renders behind player sprite

        // Match sliced draw mode if the main renderer uses it
        _outlineSr.drawMode = _sr.drawMode;
        if (_sr.drawMode == SpriteDrawMode.Sliced || _sr.drawMode == SpriteDrawMode.Tiled)
            _outlineSr.size = _sr.size;

        outlineGo.SetActive(false);
    }

    // ── Public API called by PlayerStats / PlayerCombat ───────────────────────

    public void OnHit()
    {
        if (_hitRoutine != null) StopCoroutine(_hitRoutine);
        _hitRoutine = StartCoroutine(HitFlash());
    }

    public void OnAttack(float duration = -1f)
    {
        if (_attackRoutine != null) StopCoroutine(_attackRoutine);
        float dur = duration > 0f ? duration : attackOutlineDuration;
        _attackRoutine = StartCoroutine(AttackOutline(dur));
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    private IEnumerator HitFlash()
    {
        _sr.color = Color.red;
        yield return new WaitForSeconds(hitFlashDuration);
        _sr.color  = _baseColor;
        _hitRoutine = null;
    }

    private IEnumerator AttackOutline(float duration)
    {
        // Keep outline sprite in sync with main sprite (may change at runtime when art arrives)
        _outlineSr.sprite = _sr.sprite;
        if (_sr.drawMode == SpriteDrawMode.Sliced || _sr.drawMode == SpriteDrawMode.Tiled)
            _outlineSr.size = _sr.size;

        _outlineSr.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        _outlineSr.gameObject.SetActive(false);
        _attackRoutine = null;
    }
}
