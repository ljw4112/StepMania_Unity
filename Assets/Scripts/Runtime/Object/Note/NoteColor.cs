using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Object.Note
{
    public static class NoteColor
    {
        private static Color Bit4Color = Color.red;
        private static Color Bit8Color = Color.blue;
        private static Color Bit12Color = new(102, 51, 153, 255);
        private static Color Bit16Color = Color.yellow;
        private static Color Bit24Color = Color.magenta;
        private static Color Bit32Color = Color.cyan;
        private static Color Bit48Color = Color.green;
        private static Color Bit64Color = Color.grey;

        public static readonly Dictionary<int, Color> BitColor = new()
        {
            { 4, Bit4Color },
            { 8, Bit8Color },
            { 12, Bit12Color },
            { 16, Bit16Color },
            { 24, Bit24Color },
            { 32, Bit32Color },
            { 48, Bit48Color },
            { 64, Bit64Color },
            { 96, Bit64Color },
            { 128, Bit64Color },
            { 192, Bit64Color },
        };
    }
}