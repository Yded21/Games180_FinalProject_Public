using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Games180_FinalProject_EddyZhang;

public static class UiHelper
{
    public static void DrawPanel(SpriteBatch spriteBatch, Rectangle rectangle, Color fillColor, Color borderColor)
    {
        spriteBatch.Draw(AssetManager.Pixel, rectangle, fillColor);
        DrawRectangleOutline(spriteBatch, rectangle, borderColor, 2);
    }

    public static void DrawRectangleOutline(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int thickness)
    {
        spriteBatch.Draw(AssetManager.Pixel, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
        spriteBatch.Draw(AssetManager.Pixel, new Rectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
        spriteBatch.Draw(AssetManager.Pixel, new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
        spriteBatch.Draw(AssetManager.Pixel, new Rectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), color);
    }

    public static void DrawBar(SpriteBatch spriteBatch, Rectangle rectangle, float ratio, Color fillColor, Color backColor, Color borderColor)
    {
        if (ratio < 0f)
        {
            ratio = 0f;
        }
        if (ratio > 1f)
        {
            ratio = 1f;
        }

        spriteBatch.Draw(AssetManager.Pixel, rectangle, backColor);
        int width = (int)(rectangle.Width * ratio);
        spriteBatch.Draw(AssetManager.Pixel, new Rectangle(rectangle.X, rectangle.Y, width, rectangle.Height), fillColor);
        DrawRectangleOutline(spriteBatch, rectangle, borderColor, 2);
    }

    public static void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale)
    {
        spriteBatch.DrawString(AssetManager.Font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    public static void DrawCenteredText(SpriteBatch spriteBatch, string text, Rectangle rectangle, Color color, float scale)
    {
        Vector2 textSize = AssetManager.Font.MeasureString(text) * scale;
        float textX = rectangle.X + (rectangle.Width - textSize.X) / 2f;
        float textY = rectangle.Y + (rectangle.Height - textSize.Y) / 2f;
        spriteBatch.DrawString(AssetManager.Font, text, new Vector2(textX, textY), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    public static void DrawCenteredWrappedText(SpriteBatch spriteBatch, string text, Rectangle rectangle, Color color, float maxScale, float minScale)
    {
        float scale = maxScale;
        List<string> lines = WrapText(text, rectangle.Width, scale);
        while (scale > minScale && GetWrappedHeight(lines, scale) > rectangle.Height)
        {
            scale -= 0.05f;
            lines = WrapText(text, rectangle.Width, scale);
        }

        float lineHeight = AssetManager.Font.LineSpacing * scale;
        float totalHeight = lines.Count * lineHeight;
        float y = rectangle.Y + (rectangle.Height - totalHeight) / 2f;

        for (int i = 0; i < lines.Count; i++)
        {
            Vector2 textSize = AssetManager.Font.MeasureString(lines[i]) * scale;
            float x = rectangle.X + (rectangle.Width - textSize.X) / 2f;
            spriteBatch.DrawString(AssetManager.Font, lines[i], new Vector2(x, y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            y += lineHeight;
        }
    }

    private static List<string> WrapText(string text, int maxWidth, float scale)
    {
        List<string> lines = new List<string>();
        if (string.IsNullOrEmpty(text))
        {
            return lines;
        }

        string[] words = text.Split(' ');
        string current = string.Empty;

        for (int i = 0; i < words.Length; i++)
        {
            string test = words[i];
            if (current.Length > 0)
            {
                test = current + " " + words[i];
            }

            if (AssetManager.Font.MeasureString(test).X * scale <= maxWidth || current.Length == 0)
            {
                current = test;
            }
            else
            {
                lines.Add(current);
                current = words[i];
            }
        }

        if (current.Length > 0)
        {
            lines.Add(current);
        }

        return lines;
    }

    private static float GetWrappedHeight(List<string> lines, float scale)
    {
        return lines.Count * AssetManager.Font.LineSpacing * scale;
    }
}
