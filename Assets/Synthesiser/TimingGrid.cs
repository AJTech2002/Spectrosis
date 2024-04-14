using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingGrid : MonoBehaviour
{
    public float BPM = 120;

    public int beatsPerBar = 4;
    private int beatNum = 0;
    public static event Action<int> OnBeat;

    private IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(60 / BPM / beatsPerBar);
            OnBeat?.Invoke(GetDivision(beatNum));
            beatNum = (beatNum + 1) % beatsPerBar;
        }
    }

    private int GetDivision(int beatNum)
    {
        return beatNum * beatsPerBar;
    }
}
