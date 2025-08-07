using System.Collections;
using System.Collections.Generic;
using System;

public static class Extension
{
    private static Random rng = new();  //Random Number Generator (무작위 숫자 생성기)

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}
