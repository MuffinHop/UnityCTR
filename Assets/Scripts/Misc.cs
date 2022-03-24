using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OGData.Kart
{
    public class Misc
    {
        // 
        
        /// <summary>
        /// Method MapToRange 80058f9c
        /// Maps a new range for value;
        /// </summary>
        /// <exception cref="ArithmeticException">
        /// Thrown if old maximum is zero; or min is less than max
        /// </exception>
        /// <return>
        /// Value in the new range
        /// </return>
        public static int MapToRange(int value, int oldMinimum, int oldMaximum, int newMinimum, int newMaximum)
        {
            int dist;

            // if value is lessor equal than oldMin, return newMin
            if (value <= oldMinimum) {
                return newMinimum;
            }

            // if value is less than old Max
            if (value < oldMaximum)
            {

                // distance from old min, multiplied by new range
                dist = (value - oldMinimum) * (newMaximum - newMinimum);

                // get range of first min and max
                int scale = oldMaximum - oldMinimum;

                if (scale == 0) {
                    throw new ArithmeticException("Old Min Max distance is zero");
                }

                if ((scale == -1) && (dist == -0x80000000)) {
                    throw new ArithmeticException("Max is less than Min");
                }

                // new min, plus [...] / old range
                return newMinimum + dist / scale;
            }

            // return new max
            return newMaximum;
        }
    }
}