using Steamworks;
using UnityEngine;

namespace Game.Scripts.Services.Steam
{
    public static class MySteamUtils
    {
        public static Texture2D LoadSteamImage(CSteamID playerID)
        {
            int imageID = SteamFriends.GetLargeFriendAvatar(playerID);
            if (imageID == -1) return null;
            uint width, height;
            bool ok = SteamUtils.GetImageSize(imageID, out width, out height);
            if (!ok || width == 0 || height == 0) return null;

            byte[] rawData = new byte[width * height * 4];
            ok = SteamUtils.GetImageRGBA(imageID, rawData, (int)(width * height * 4));
            if (!ok) return null;

            // Создаём Texture2D
            Texture2D tex = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);

            // Разворачиваем изображение по вертикали
            Color32[] pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int src = (int)((y * width + x) * 4);
                    Color32 c = new Color32(rawData[src], rawData[src + 1], rawData[src + 2], rawData[src + 3]);
                    pixels[(int)((height - 1 - y) * width + x)] = c;
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            return tex;
        }

    
    }
}