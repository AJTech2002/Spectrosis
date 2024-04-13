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

    //private bool arpeggiatorEnabled;
    private uint arpeggiatorSpeed;
    private int arpeggiatorIndex;

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

    private List<int> activeNotes = new List<int>();

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
                activeNotes.Add(x);
                arpeggiatorIndex = 0;
                if (debugImmidiatePlay)
                {
                    Play(new PlayData
                    {
                        cutoff = 3000,
                        attack = 0.1f,
                        decay = 0.1f,
                    });
                }
            }
            else if (Input.GetKeyUp(keyboardKeys[x]))
            {
                if (debugImmidiatePlay)
                {
                    Stop();
                }
                activeNotes.Remove(x);
            }
        }

        arpeggiatorSpeed += Input.mouseScrollDelta.y > 0 ? 1u : Input.mouseScrollDelta.y < 0 ? 0u : arpeggiatorSpeed;
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

    public void SetArpeggiatorSpeed(uint speed)
    {
        if (this.arpeggiatorSpeed == 0 && speed != 0)
        {
            arpeggiatorIndex = 0;
        }
        this.arpeggiatorSpeed = speed;
    }

    private void OnBeat(int beatNum)
    {
        if (beatNum - arpeggiatorSpeed < 0) return;
        if (arpeggiatorSpeed != 0 && activeNotes.Count > 0)
        {
            synth.NoteUp(activeNotes[0] + arpeggiatorIndex, activeNotes[0] + arpeggiatorIndex);
            arpeggiatorIndex = (arpeggiatorIndex + 2) % 8;
            int noteIndex = activeNotes[0] + arpeggiatorIndex;
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

        foreach (int note in activeNotes)
        {
            synth.NoteDown(note, note);
        }
    }

    public void Stop()
    {
        foreach (int note in activeNotes)
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
