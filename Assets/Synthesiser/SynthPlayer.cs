using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synth;
using Synth.Module;

[System.Serializable]
public struct PlayData
{
    public int cutoff;
    //public float resonance;
    public float attack;
    public float decay;
    public float tremoloFrequency;
    public float tremoloAmplitude;
    public float sustain;
    public float release;
    public float vol;
    public float reverb;
    public int octave;
    public float chorus;
    public float damping;
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

    private bool isPlaying;

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
    };

    private readonly KeyCode[] octaveKeys =
    {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
    };

    private List<int> activeKeys = new List<int>();

    private void Awake()
    {
        synth = new Controller();
        synth.Osc1Enable = true;
        synth.Osc1Waveform = Synth.Module.SignalType.Sine;

        synth.FilterEnable = true;

        synth.chorusModule.Enabled = false;
        synth.chorusModule.Delay = 0.1f;
        synth.chorusModule.SweepRate = 0.2f;
        synth.chorusModule.Width = 0.2f;

        synth.reverbModule.Enabled = true;

        synth.DelayEnable = true;
        synth.Decay = 0.4f;
        synth.DelayDry = 1;
        synth.DelayWet = 0;
        synth.Delay = 0.4f;

        synth.TremoloEnable = false;
        synth.DistortEnable = false;

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

                if (debugImmidiatePlay || isPlaying)
                {
                    synth.NoteDown(x, x);
                }
            }
            else if (Input.GetKeyUp(keyboardKeys[x]))
            {
                if (debugImmidiatePlay || isPlaying)
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

        for (int x = 0; x < octaveKeys.Length; x++)
        {
            if (Input.GetKeyDown(octaveKeys[x]))
            {
                synth.Osc1Octave = x;
            }
        }

        SetArpeggiatorSpeed(Input.mouseScrollDelta.y > 0 ? Mathf.Clamp(arpeggiatorSpeed + 1, 0, arpeggiatorBeatDivisions.Length - 1) : Input.mouseScrollDelta.y < 0 ? Mathf.Clamp(arpeggiatorSpeed - 1, 0, arpeggiatorBeatDivisions.Length - 1) : arpeggiatorSpeed);
    }
    
    public void UpdateSynth(PlayData playData, float delta)
    {
        synth.Osc1Volume = playData.vol;

        synth.Attack = playData.attack;
        synth.Decay = playData.decay;
        synth.Sustain = playData.sustain;
        synth.Release = playData.release;

        //synth.FilterType = playData.cutoff > 800f ? Synth.Filter.FilterType.HighPass : Synth.Filter.FilterType.LowPass;
        //synth.Cutoff = playData.cutoff;

        synth.reverbModule.DryWet = playData.reverb;
        synth.reverbModule.RoomSize = playData.reverb;
        //synth.Osc1Octave = playData.octave;

        synth.chorusModule.Enabled = playData.chorus > 0.1f;
        synth.chorusModule.DryWet = playData.chorus;

        synth.DelayDry = 1 - playData.damping;
        synth.DelayWet = playData.damping;
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

    public void Play()
    {
/*        //Reverb here at some point
        UpdateSynth(playData, 0f);
        
        //Distortion too here at some point*/

        foreach (int note in activeKeys)
        {
            synth.NoteDown(note, note);
        }
        isPlaying = true;
    }

    public void Stop()
    {
        isPlaying = false;
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
