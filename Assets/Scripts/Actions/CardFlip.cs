using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class CardFlip : MonoBehaviour
{
    public float flipDuration = 0.3f;

    private RectTransform _rt;

    private void Awake() => _rt = GetComponent<RectTransform>();

    public void StartFlip(bool toFaceUp, CardDisplay display)
    {
        StopAllCoroutines();
        StartCoroutine(FlipRoutine(toFaceUp, display));
    }

    private IEnumerator FlipRoutine(bool toFaceUp, CardDisplay display)
    {
        // 1. Shrink X to 0
        float half = flipDuration * 0.5f;
        float t = 0;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float norm = t / half;
            _rt.localScale = new Vector3(1f - norm, 1f, 1f);
            yield return null;
        }

        // 2. Swap side at the midpoint
        display.SetFaceUp(toFaceUp);

        // 3. Grow X back
        t = 0;
        _rt.localScale = new Vector3(0f, 1f, 1f);
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float norm = t / half;
            _rt.localScale = new Vector3(norm, 1f, 1f);
            yield return null;
        }

        _rt.localScale = Vector3.one;
    }
}