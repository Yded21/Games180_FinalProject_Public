using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Games180_FinalProject_EddyZhang;

public class UIManager
{
    public void DrawHud(SpriteBatch spriteBatch, Player player, RoomDefinition room, int roomIndex, int roomCount, List<UpgradeDefinition> upgrades, BossEnemy boss)
    {
        Rectangle hud = new Rectangle(0, 0, Game1.ScreenWidth, Game1.HudHeight);
        UiHelper.DrawPanel(spriteBatch, hud, new Color(28, 28, 32), new Color(12, 12, 14));

        UiHelper.DrawBar(spriteBatch, new Rectangle(20, 20, 220, 24), (float)player.CurrentHp / player.MaxHp, new Color(180, 40, 40), new Color(55, 20, 20), Color.Black);
        UiHelper.DrawText(spriteBatch, "HP " + player.CurrentHp + " / " + player.MaxHp, new Vector2(20, 50), Color.White, 0.8f);
        Texture2D formIcon = GetFormIcon(player.Form);
        Color formColor = Color.White;
        string formName = player.Form.ToString();
        if (!player.HasForm)
        {
            formColor = Color.White * 0.45f;
            formName = "No Form";
        }

        spriteBatch.Draw(formIcon, new Rectangle(248, 18, 40, 40), formColor);
        UiHelper.DrawText(spriteBatch, formName, new Vector2(246, 64), Color.Gainsboro, 0.75f);

        DrawProgress(spriteBatch, roomIndex, roomCount, room.Name);

        DrawCooldown(spriteBatch, AssetManager.UpgradeIcons[UpgradeCategory.Speed], new Rectangle(580, 18, 36, 36), player.DashCooldownRatio);
        Color skillColor = Color.White;
        if (!player.HasForm)
        {
            skillColor = Color.White * 0.35f;
        }
        DrawCooldown(spriteBatch, GetSkillIcon(player), new Rectangle(626, 18, 36, 36), player.SkillCooldownRatio, skillColor);

        for (int i = 0; i < upgrades.Count && i < 16; i++)
        {
            Texture2D icon = AssetManager.UpgradeIcons[upgrades[i].Category];
            int x = 580 + (i % 12) * 28;
            int y = 70 + (i / 12) * 24;
            spriteBatch.Draw(icon, new Rectangle(x, y, 22, 22), Color.White);
        }

        if (boss != null && !boss.IsDead)
        {
            UiHelper.DrawCenteredText(spriteBatch, "BOSS", new Rectangle(0, 118, Game1.ScreenWidth, 18), Color.White, 0.7f);
            UiHelper.DrawBar(spriteBatch, new Rectangle(250, 136, 460, 18), (float)boss.Hp / boss.MaxHp, new Color(140, 30, 50), new Color(38, 14, 20), Color.Black);
        }
    }

    public void DrawPause(SpriteBatch spriteBatch, Player player, RoomDefinition room, List<UpgradeDefinition> upgrades, int pauseSelection, bool saveExists)
    {
        DrawOverlay(spriteBatch);
        UiHelper.DrawPanel(spriteBatch, new Rectangle(130, 70, 700, 520), new Color(34, 34, 40), Color.Gray);
        UiHelper.DrawText(spriteBatch, "Paused", new Vector2(170, 105), Color.White, 1.1f);
        UiHelper.DrawText(spriteBatch, "HP: " + player.CurrentHp + " / " + player.MaxHp, new Vector2(170, 160), Color.Gainsboro, 0.8f);
        string formText = player.Form.ToString();
        if (!player.HasForm)
        {
            formText = "None";
        }
        UiHelper.DrawText(spriteBatch, "Form: " + formText, new Vector2(170, 195), Color.Gainsboro, 0.8f);
        UiHelper.DrawText(spriteBatch, "Room: " + room.Name, new Vector2(170, 230), Color.Gainsboro, 0.8f);
        UiHelper.DrawText(spriteBatch, "Upgrades", new Vector2(470, 105), Color.White, 0.9f);

        for (int i = 0; i < upgrades.Count && i < 12; i++)
        {
            spriteBatch.Draw(AssetManager.UpgradeIcons[upgrades[i].Category], new Rectangle(470, 148 + i * 26, 20, 20), Color.White);
            UiHelper.DrawText(spriteBatch, upgrades[i].Name, new Vector2(500, 146 + i * 26), Color.Gainsboro, 0.65f);
        }

        Rectangle saveBtn = new Rectangle(175, 510, 200, 44);
        Rectangle loadBtn = new Rectangle(585, 510, 200, 44);
        Color saveBorder = pauseSelection == 0 ? new Color(230, 200, 90) : new Color(80, 80, 90);
        Color loadBorder = pauseSelection == 1 ? new Color(230, 200, 90) : new Color(80, 80, 90);
        Color loadTextColor = saveExists ? Color.White : Color.Gray;

        UiHelper.DrawPanel(spriteBatch, saveBtn, new Color(44, 44, 52), saveBorder);
        UiHelper.DrawPanel(spriteBatch, loadBtn, new Color(44, 44, 52), loadBorder);
        UiHelper.DrawCenteredText(spriteBatch, "Save Game  [T]", saveBtn, Color.White, 0.72f);
        UiHelper.DrawCenteredText(spriteBatch, "Load Game  [Y]", loadBtn, loadTextColor, 0.72f);
        UiHelper.DrawCenteredText(spriteBatch, "A / D to select   E to confirm", new Rectangle(130, 558, 700, 26), new Color(130, 130, 140), 0.65f);
    }

    private void DrawProgress(SpriteBatch spriteBatch, int roomIndex, int roomCount, string roomName)
    {
        int startX = 320;
        for (int i = 0; i < roomCount; i++)
        {
            Color color = new Color(65, 65, 70);
            if (i < roomIndex)
            {
                color = new Color(160, 145, 95);
            }
            if (i == roomIndex)
            {
                color = new Color(230, 210, 110);
            }

            spriteBatch.Draw(AssetManager.Pixel, new Rectangle(startX + i * 24, 32, 16, 16), color);
        }
        UiHelper.DrawCenteredText(spriteBatch, roomName, new Rectangle(300, 62, 260, 28), Color.Gainsboro, 0.68f);
    }

    private void DrawCooldown(SpriteBatch spriteBatch, Texture2D icon, Rectangle rect, float ratio)
    {
        DrawCooldown(spriteBatch, icon, rect, ratio, Color.White);
    }

    private void DrawCooldown(SpriteBatch spriteBatch, Texture2D icon, Rectangle rect, float ratio, Color color)
    {
        spriteBatch.Draw(icon, rect, color);
        if (ratio > 0f)
        {
            int overlayHeight = (int)(rect.Height * ratio);
            spriteBatch.Draw(AssetManager.Pixel, new Rectangle(rect.X, rect.Y, rect.Width, overlayHeight), Color.Black * 0.6f);
        }
    }

    private void DrawOverlay(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(AssetManager.Pixel, new Rectangle(0, 0, Game1.ScreenWidth, Game1.ScreenHeight), Color.Black * 0.65f);
    }

    private Texture2D GetSkillIcon(Player player)
    {
        if (player.Form == PlayerForm.Knight)
        {
            return AssetManager.UpgradeIcons[UpgradeCategory.Knight];
        }
        if (player.Form == PlayerForm.Hero)
        {
            return AssetManager.UpgradeIcons[UpgradeCategory.Hero];
        }
        if (player.Form == PlayerForm.Divine)
        {
            return AssetManager.UpgradeIcons[UpgradeCategory.Divine];
        }
        return AssetManager.UpgradeIcons[UpgradeCategory.Demon];
    }

    private Texture2D GetFormIcon(PlayerForm form)
    {
        if (form == PlayerForm.Knight)
        {
            return AssetManager.UpgradeIcons[UpgradeCategory.Knight];
        }
        if (form == PlayerForm.Hero)
        {
            return AssetManager.UpgradeIcons[UpgradeCategory.Hero];
        }
        if (form == PlayerForm.Divine)
        {
            return AssetManager.UpgradeIcons[UpgradeCategory.Divine];
        }
        return AssetManager.UpgradeIcons[UpgradeCategory.Demon];
    }

}
