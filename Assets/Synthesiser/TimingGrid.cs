using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingGrid : MonoBehaviour
{
    public float BPM = 120;

    private const int MIN_BEAT_DIVISION = 32;
    private int beatNum = 1;
    public static event Action<int> OnBeat;

    private float timer;
    private float interval;

    private void Awake()
    {
        interval = 60f / BPM / MIN_BEAT_DIVISION;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > interval)
        {
            timer -= interval;
            OnBeat?.Invoke(GetDivision(beatNum));
            beatNum = beatNum == MIN_BEAT_DIVISION ? 1 : beatNum + 1;
        }
    }

    private int GetDivision(int beatNum)
    {
        if (beatNum % 16 == 0)
            return 2;

        if (beatNum % 8 == 0)
            return 4;

        if (beatNum % 4 == 0)
            return 8;

        if (beatNum % 2 == 0)
            return 16;

        return 32;
    }

/*    public bool IsBeatEquivalent(int beatDivision, int currentDivision)
    {
        
    }*/
}
