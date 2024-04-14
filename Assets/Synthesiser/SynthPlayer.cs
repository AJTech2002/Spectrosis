using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synth;

public struct PlayData
{
    public int cutoff;
    //public float resonance;
    public float attack;
    public float decay;
    public float tremoloFrequency;
    public float tremoloAmplitude;
}

public class SynthPlayer : MonoBehaviour
{
    [SerializeField]
    private bool debugImmidiatePlay;

    private int arpeggiatorSpeed;
    private int arpeggiatorIndex;
    private int arpeggiatorRoot;

    private readonly int[] arpeggiatorBeatDivisions = { 0, 2, 4, 16, 32 };

    private readonly int[] arpeggiatorIntervals = { 0, 2, 4, 6 };

    private Controller synth;

    private readonly KeyCode[] keyboardKeys =
    {
        KeyCode.A,
        KeyCode.S,
        KeyCode.D,
        KeyCode.F,
        KeyCode.G,
        KeyCode.H,
        KeyCode.J,
        KeyCode.K,
        KeyCode.L,
    };

    private List<int> activeKeys = new List<int>();

    private void Awake()
    {
        synth = new Controller();
        synth.Osc1Enable = true;
        synth.Osc1Waveform = Synth.Module.SignalType.Sine;
        synth.Dry = 0.2f;
        synth.Wet = 0.8f;

/*        synth.reverbModule.RoomSize = 0.9f;
        synth.reverbModule.Damping = 0.1f;
        //synth.reverbModule.DryWet = 1;
        synth.reverbModule.Enabled = false;
        synth.chorusModule.Delay = 0.2f;
        synth.chorusModule.Width = 0.2f;
        synth.chorusModule.SweepRate = 0.5f;

        synth.chorusModule.Enabled = false;

        synth.TremoloAmplitude = 40f;
        synth.TremoloFrequency = 10;
        synth.TremoloEnable = false;*/

        TimingGrid.OnBeat += OnBeat;
    }

    private void Update()
    {
        for (int x = 0; x < keyboardKeys.Length; x++)
        {
            if (Input.GetKeyDown(keyboardKeys[x]))
            {
                if (!activeKeys.Contains(x))
                    activeKeys.Add(x);

                // Reset arpeggiator root
                arpeggiatorIndex = 0;
                arpeggiatorRoot = x;

                if (debugImmidiatePlay)
                {
                    synth.NoteDown(x, x);
                }
            }
            else if (Input.GetKeyUp(keyboardKeys[x]))
            {
                if (debugImmidiatePlay)
                {
                    synth.NoteUp(x, x);
                }

                // If arpeggiator is active and the key being released is the root note, stop the arpeggiator
                if (arpeggiatorSpeed != 0 && x == arpeggiatorRoot)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        synth.NoteUp(i, i);
                    }
                }
                activeKeys.Remove(x);
            }
        }

        SetArpeggiatorSpeed(Input.mouseScrollDelta.y > 0 ? Mathf.Clamp(arpeggiatorSpeed + 1, 0, arpeggiatorBeatDivisions.Length - 1) : Input.mouseScrollDelta.y < 0 ? Mathf.Clamp(arpeggiatorSpeed - 1, 0, arpeggiatorBeatDivisions.Length - 1) : arpeggiatorSpeed);
    }
    
    private void UpdateSynth(PlayData playData)
    {
        synth.Cutoff = playData.cutoff;
        synth.Attack = playData.attack;
        synth.Decay = playData.decay;
        synth.Sustain = 1;
        synth.Release = 0.3f;
        /*synth.TremoloFrequency = playData.tremoloFrequency;
        synth.TremoloAmplitude = playData.tremoloAmplitude;*/
    }

    public void SetArpeggiatorSpeed(int speed)
    {
        if (activeKeys.Count == 0) return;
        //Arp started
        if (this.arpeggiatorSpeed == 0 && speed != 0)
        {
            arpeggiatorIndex = 0;
            arpeggiatorRoot = activeKeys[0];
        }
        //Arp stopped
        else if (this.arpeggiatorSpeed != 0 && speed == 0)
        {
            for (int x = 0; x < 12; x++) 
            { 
                synth.NoteUp(x, x); 
            }

            //If still holding go back to root note
            if (activeKeys.Count > 0)
            {
                synth.NoteDown(arpeggiatorRoot, arpeggiatorRoot);
            }
        }
        this.arpeggiatorSpeed = speed;
    }

    private void OnBeat(int beatDivision)
    {
        if (arpeggiatorBeatDivisions[arpeggiatorSpeed] < beatDivision) return;
        if (arpeggiatorSpeed != 0 && activeKeys.Count > 0)
        {
            int prevNoteIndex = arpeggiatorRoot + arpeggiatorIntervals[arpeggiatorIndex] >= 8 ? (arpeggiatorRoot + arpeggiatorIntervals[arpeggiatorIndex]) % 8 : arpeggiatorRoot + arpeggiatorIntervals[arpeggiatorIndex];
            synth.NoteUp(prevNoteIndex, prevNoteIndex);

            arpeggiatorIndex = arpeggiatorIndex >= arpeggiatorIntervals.Length - 1 ? 0 : arpeggiatorIndex + 1;
            int noteIndex = arpeggiatorRoot + arpeggiatorIntervals[arpeggiatorIndex] >= 8 ? (arpeggiatorRoot + arpeggiatorIntervals[arpeggiatorIndex]) % 8 : arpeggiatorRoot + arpeggiatorIntervals[arpeggiatorIndex];

            synth.NoteDown(noteIndex, noteIndex);
        }
    }

    // Mouse wobble
    public void SetWobble(int frequency, float amplitude)
    {
        synth.TremoloEnable = true;
        synth.TremoloFrequency = frequency;
        synth.TremoloAmplitude = amplitude;
    }

    public void Play(PlayData playData)
    {
       
        //Reverb here at some point
        UpdateSynth(playData);
        
        //Distortion too here at some point

        foreach (int note in activeKeys)
        {
            synth.NoteDown(note, note);
        }
    }

    public void Stop()
    {
        foreach (int note in activeKeys)
        {
            synth.NoteUp(note, note);
        }
    }

    private void OnDestroy()
    {
        if (synth != null)
        {
            // Destroys synth threads
            synth.Osc1Enable = false;
            synth.Osc2Enable = false;
        }
    }
}
