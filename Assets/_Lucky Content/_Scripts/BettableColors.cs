using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucky
{
    /// <summary>
    /// The colors that can be bet upon.
    /// If you add to this, also add a case in the below extension class for the analagous colours.
    /// </summary>
    public enum BettableColors : byte
    {
        Red,
        Green
    } 

    public static class BettableColorExtensions
    {
        public static Color ToColor(this BettableColors bettableColor)
        {
            switch (bettableColor) {
                case BettableColors.Red:
                    return Color.red;
                case BettableColors.Green:
                    return Color.green;
            }

            return Color.white;
        }
    }
}
