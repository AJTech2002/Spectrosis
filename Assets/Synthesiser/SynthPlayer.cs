using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using UnityEngine;
using Synth;
using Synth.Filter;

public class SynthPlayer : MonoBehaviour
{
    [SerializeField]
    private bool debugImmidiatePlay;

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

    public struct PlayData
    {
        public int cutoff;
        //public float resonance;
        public float attack;
        public float decay;
        public float sustain;
        public float release;

        public float volume;
    }

    private void Awake()
    {
        synth = new Controller();
        synth.Osc1Enable = true;
        synth.Osc1Waveform = Synth.Module.SignalType.Sine;
        synth.Dry = 0.2f;
        synth.Wet = 0.8f;
    }

    private void Update()
    {
        for (int x = 0; x < keyboardKeys.Length; x++)
        {
            if (Input.GetKeyDown(keyboardKeys[x]))
            {
                activeNotes.Add(x);
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
        // How many times a ray hit a wall minus some factor for escaping if it does
        synth.FilterType = Synth.Filter.FilterType.LowPass;
        synth.FilterEnable = true;
        synth.Cutoff = playData.cutoff;

        // Material, if i can find how to set it
        //synth.Resonance

        // Material
        synth.Decay = playData.decay;
        synth.Attack = playData.attack;
        synth.Sustain = playData.sustain;
        synth.Release = playData.release;

        synth.Osc1Volume = playData.volume;

        //Reverb here at some point

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
