using UnityEngine;
using UnityEngine.UI;

public class GradientBackground : MonoBehaviour
{
    public Color topColor = new Color(0.10f, 0.10f, 0.14f, 1f); // Enhanced slate-900
    public Color middleColor = new Color(0.50f, 0.12f, 0.12f, 1f); // Enhanced red-950
    public Color bottomColor = new Color(0.02f, 0.02f, 0.02f, 1f); // Slightly lighter black for depth

    void Start()
    {
        CreateGradient();
    }

    void CreateGradient()
    {
        // Enhanced gradient with higher resolution for smoother transitions
        Texture2D texture = new Texture2D(1, 512);

        for (int i = 0; i < 512; i++)
        {
            float t = i / 512f;
            Color color;

            // Enhanced transition with smoothstep for more natural gradient
            if (t < 0.4f)
            {
                // Top to middle transition (slower, more gradual)
                float smoothT = Mathf.SmoothStep(0f, 1f, t / 0.4f);
                color = Color.Lerp(topColor, middleColor, smoothT);
            }
            else if (t < 0.7f)
            {
                // Middle section (slight pause in transition for richer middle tone)
                float smoothT = Mathf.SmoothStep(0f, 1f, (t - 0.4f) / 0.3f);
                color = Color.Lerp(middleColor, new Color(middleColor.r * 0.85f, middleColor.g * 0.85f, middleColor.b * 0.85f, middleColor.a), smoothT);
            }
            else
            {
                // Middle to bottom transition
                float smoothT = Mathf.SmoothStep(0f, 1f, (t - 0.7f) / 0.3f);
                Color darkerMiddle = new Color(middleColor.r * 0.85f, middleColor.g * 0.85f, middleColor.b * 0.85f, middleColor.a);
                color = Color.Lerp(darkerMiddle, bottomColor, smoothT);
            }

            texture.SetPixel(0, i, color);
        }

        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        GetComponent<Image>().sprite = Sprite.Create(texture,
            new Rect(0, 0, 1, 512), Vector2.zero);
    }
}