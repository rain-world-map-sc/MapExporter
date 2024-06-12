﻿using System.Collections;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using static MapExporter.Generation.GenUtil;

namespace MapExporter.Generation
{
    internal class MiscProcessor(Generator owner) : Processor(owner)
    {
        public override string ProcessName => "Misc";

        protected override IEnumerator Process()
        {
            var metadata = owner.metadata;
            var regionInfo = owner.regionInfo;

            // Find colors
            Color fgcolor = Mode(regionInfo.fgcolors);
            Color bgcolor = Mode(regionInfo.bgcolors);
            Color sccolor = Mode(regionInfo.sccolors);

            metadata["bgcolor"] = Color2Arr(fgcolor);
            metadata["highlightcolor"] = Color2Arr(bgcolor);
            metadata["shortcutcolor"] = Color2Arr(sccolor);

            // Calculate a geo color
            Vector3 bvec = HSL2HSV(Custom.RGB2HSL(bgcolor));
            Vector3 fvec = HSL2HSV(Custom.RGB2HSL(fgcolor));
            var (bh, bs, bv) = (bvec.x, bvec.y, bvec.z);
            var (fh, fs, fv) = (fvec.x, fvec.y, fvec.z);
            float sh, ss, sv;

            if (Mathf.Abs(bh - fh) < 0.5f)
            {
                if (bh < fh)
                    bh += 1;
                else
                    fh += 1;
            }
            sh = (bs == 0 && fs == 0) ? 0.5f : ((bh * fs + fh * bs) / (bs + fs));
            sh = sh < 0 ? (1 + (sh % 1f)) : (sh % 1f);

            ss = Mathf.Sqrt((bs * bs + fs * fs) / 2.0f); // this does some circle math stuff
            sv = Mathf.Sqrt((bv * bv + fv * fv) / 2.0f); // ditto

            metadata["geocolor"] = Color2Arr(HSV2HSL(sh, ss, sv).rgb);

            Progress = 1f;
            yield return null;
            Done = true;
        }

        private static Color Mode(List<Color> colors)
        {
            Dictionary<Color, int> map = [];
            int max = 0;
            Color maxColor = Color.black;
            for (int i = 0; i < colors.Count; i++)
            {
                var color = colors[i];
                if (map.ContainsKey(color))
                {
                    map[color]++;
                }
                else
                {
                    map.Add(color, 0);
                }

                if (map[color] > max)
                {
                    max = map[color];
                    maxColor = color;
                }
            }

            return maxColor;
        }

        private static Vector3 HSL2HSV(Vector3 hsl)
        {
            var (h, s, l) = (hsl.x, hsl.y, hsl.z);
            float v = l + s * Mathf.Min(l, 1 - l);
            return new Vector3(h, v == 0f ? 0f : 2 * (1 - l / v), v);
        }

        private static HSLColor HSV2HSL(float h, float s, float v)
        {
            float l = v * (1f - s / 2f);
            return new HSLColor(h, (l == 0 || l == 1) ? 0f : ((v - l) / Mathf.Min(l, 1 - l)), l);
        }
    }
}
