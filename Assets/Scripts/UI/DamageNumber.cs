using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Spawned over enemies when they take damage. Floats upward and fades out.
/// </summary>
public class DamageNumber : MonoBehaviour
{
    private TextMeshPro _tmp;

    private void Awake()
    {
        _tmp = GetComponent<TextMeshPro>();
    }

    public void Show(float damage, bool isCrit, Vector3 worldPos)
    {
        transform.position = worldPos + new Vector3(Random.Range(-0.15f, 0.15f), 0.3f, 0f);

        if (isCrit)
        {
            _tmp.text      = $"<b>{Mathf.RoundToInt(damage)}!</b>";
            _tmp.fontSize  = 5f;
            _tmp.color     = new Color(1f, 0.85f, 0f);
        }
        else
        {
            _tmp.text      = Mathf.RoundToInt(damage).ToString();
            _tmp.fontSize  = 3.5f;
            _tmp.color     = Color.white;
        }

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float duration = 0.8f;
        float elapsed  = 0f;
        Vector3 startPos = transform.position;
        Color   startCol = _tmp.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.position = startPos + new Vector3(0f, t * 1.2f, 0f);
            _tmp.color = new Color(startCol.r, startCol.g, startCol.b, 1f - t);

            yield return null;
        }

        Destroy(gameObject);
    }
}
