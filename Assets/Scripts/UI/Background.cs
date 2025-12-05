using UnityEngine;
using UnityEngine.UI;

public class GradientBackground : MonoBehaviour
{
    public Color topColor = new Color(0.07f, 0.07f, 0.11f); // Slate-900
    public Color middleColor = new Color(0.45f, 0.09f, 0.09f); // Red-950
    public Color bottomColor = new Color(0f, 0f, 0f); // Black

    void Start()
    {
        CreateGradient();
    }

    void CreateGradient()
    {
        Texture2D texture = new Texture2D(1, 256);

        for (int i = 0; i < 256; i++)
        {
            float t = i / 256f;
            Color color;

            if (t < 0.5f)
            {
                color = Color.Lerp(topColor, middleColor, t * 2f);
            }
            else
            {
                color = Color.Lerp(middleColor, bottomColor, (t - 0.5f) * 2f);
            }

            texture.SetPixel(0, i, color);
        }

        texture.Apply();
        GetComponent<Image>().sprite = Sprite.Create(texture,
            new Rect(0, 0, 1, 256), Vector2.zero);
    }
}