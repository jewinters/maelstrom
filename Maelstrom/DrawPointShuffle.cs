﻿using System;

namespace FF8Mod.Maelstrom
{
    public static class DrawPointShuffle
    {
        // assign random spells to each draw point, retaining their other properties
        public static BinaryPatch Patch
        {
            get
            {
                var random = new Random();
                var spells = (int[])Enum.GetValues(typeof(DrawPoint.Magic));

                for (int i = 0; i < DrawPoint.DrawPointDefsLength; i++)
                {
                    var drawPoint = new DrawPoint(DrawPoint.OriginalData[i])
                    {
                        Spell = (DrawPoint.Magic)spells[random.Next(spells.Length)]
                    };
                    DrawPoint.UpdatedDrawPoints[i + 1] = drawPoint;
                }

                return new BinaryPatch(DrawPoint.DrawPointDefsLocation, DrawPoint.OriginalData, DrawPoint.CurrentData);
            }
        }
    }
}
