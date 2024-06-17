﻿using RWCustom;
using Unity.Collections;
using UnityEngine;

namespace MapExporter.Generation
{
    internal static class GenUtil
    {
        public static IntVector2 Vec2IntVecFloor(Vector2 v) => new(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
        public static IntVector2 Vec2IntVecCeil(Vector2 v) => new(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));
        public static float[] Vec2arr(Vector2 vec) => [vec.x, vec.y];
        public static float[] Color2Arr(Color vec) => [vec.r, vec.g, vec.b];
        public static float[][] Rect2Arr(Rect rect) => [
                Vec2arr(new Vector2(rect.xMin, rect.yMin)),
                Vec2arr(new Vector2(rect.xMin, rect.yMax)),
                Vec2arr(new Vector2(rect.xMax, rect.yMax)),
                Vec2arr(new Vector2(rect.xMax, rect.yMin)),
                Vec2arr(new Vector2(rect.xMin, rect.yMin))
            ];

        public static void ScaleTexture(Texture2D texture, int width, int height)
        {
            int oldW = texture.width, oldH = texture.height;
            var oldPixels = texture.GetRawTextureData<Color32>();

            // Create the new texture
            texture.Resize(width, height);
            var pixels = new NativeArray<Color32>(width * height, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            // Use bilinear filtering
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float u = Custom.LerpMap(x, 0, width - 1, 0, oldW - 1);
                    float v = Custom.LerpMap(y, 0, height - 1, 0, oldH - 1);

                    Color tl = oldPixels[Mathf.FloorToInt(u) + Mathf.FloorToInt(v) * oldW];
                    Color tr = oldPixels[Mathf.CeilToInt(u) + Mathf.FloorToInt(v) * oldW];
                    Color bl = oldPixels[Mathf.FloorToInt(u) + Mathf.CeilToInt(v) * oldW];
                    Color br = oldPixels[Mathf.CeilToInt(u) + Mathf.CeilToInt(v) * oldW];
                    pixels[x + y * width] = Color32.LerpUnclamped(Color32.LerpUnclamped(tl, tr, u % 1f), Color32.LerpUnclamped(bl, br, u % 1f), v % 1f);
                }
            }

            // Set the new texture's content
            texture.SetPixelData(pixels, 0);
            pixels.Dispose();
        }

        public static void CopyTextureSegment(Texture2D source, Texture2D destination, int sx, int sy, int sw, int sh, int dx, int dy)
        {
            var sp = source.GetRawTextureData<Color32>();
            var dp = destination.GetRawTextureData<Color32>();

            for (int i = 0; i < sw; i++)
            {
                if (sx + i < 0 || sx + i >= source.width || dx + i < 0 || dx + i >= destination.width) continue;
                for (int j = 0; j < sh; j++)
                {
                    if (sy + j < 0 || sy + j >= source.height || dy + j < 0 || dy + j >= destination.height) continue;
                    dp[(i + dx) + (j + dy) * destination.width] = sp[(i + sx) + (j + sy) * source.width];
                }
            }

            destination.SetPixelData(dp, 0);
            sp.Dispose();
            dp.Dispose();
        }
    }
}