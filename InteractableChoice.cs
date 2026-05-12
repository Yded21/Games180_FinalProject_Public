using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Games180_FinalProject_EddyZhang;

public enum ChoiceKind
{
    StarterForm,
    Upgrade,
    Form,
    Event
}

public class InteractableChoice
{
    public ChoiceKind Kind;
    public UpgradeDefinition Upgrade;
    public PlayerForm Form;
    public int EventChoice;
    public Vector2 Position;
    public Texture2D Icon;
    public string Title;
    public string Description;

    public Rectangle InteractionBounds
    {
        get
        {
            return new Rectangle((int)Position.X - 48, (int)Position.Y - 42, 96, 84);
        }
    }

    public InteractableChoice(ChoiceKind kind, Vector2 position, Texture2D icon, string title, string description)
    {
        Kind = kind;
        Position = position;
        Icon = icon;
        Title = title;
        Description = description;
    }

    public bool IsPlayerNear(Player player)
    {
        return InteractionBounds.Intersects(player.CollisionBounds);
    }

    public void Draw(SpriteBatch spriteBatch, bool selected)
    {
        Rectangle iconRect = new Rectangle((int)Position.X - 24, (int)Position.Y - 24, 48, 48);
        Rectangle textPanel = new Rectangle((int)Position.X - 118, (int)Position.Y + 34, 236, 100);
        Color border = new Color(90, 90, 100);
        if (selected)
        {
            border = new Color(230, 205, 95);
        }

        spriteBatch.Draw(AssetManager.Pixel, new Rectangle((int)Position.X - 32, (int)Position.Y - 32, 64, 64), Color.Black * 0.35f);
        UiHelper.DrawRectangleOutline(spriteBatch, new Rectangle((int)Position.X - 32, (int)Position.Y - 32, 64, 64), border, 2);
        spriteBatch.Draw(Icon, iconRect, Color.White);
        UiHelper.DrawPanel(spriteBatch, textPanel, new Color(28, 28, 34) * 0.92f, border);
        UiHelper.DrawCenteredText(spriteBatch, Title, new Rectangle(textPanel.X + 6, textPanel.Y + 6, textPanel.Width - 12, 22), Color.White, 0.62f);
        UiHelper.DrawCenteredWrappedText(spriteBatch, Description, new Rectangle(textPanel.X + 8, textPanel.Y + 32, textPanel.Width - 16, 58), Color.Gainsboro, 0.5f, 0.33f);

        if (selected)
        {
            UiHelper.DrawCenteredText(spriteBatch, "Press E", new Rectangle(textPanel.X, textPanel.Bottom + 4, textPanel.Width, 18), Color.White, 0.55f);
        }
    }
}
