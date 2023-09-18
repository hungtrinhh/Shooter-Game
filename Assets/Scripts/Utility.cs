using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static T[] ShuffleArray<T> (T[] array, int seed) {
        System.Random prng = new System.Random (seed);

        for (int i = 0; i < array.Length - 1; i++) {
            int randomIndex = prng.Next (i, array.Length);
            T temp = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = temp;
        }

        return array;
    }

    //Returns a point on a parabol going from 0 to 1 and back to 0
    public static float InterpolateOnParabol (float percent) {
        return 4 * (-Mathf.Pow (percent, 2) + percent);
    }
}
