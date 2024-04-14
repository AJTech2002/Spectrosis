using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synth
{
    public class ChorusModule : ISampleProvider
    {
        private const int BufferSize = 8192;

        private readonly ISampleProvider _source;
        private float[] _circularBuffer = new float[BufferSize];
        private int _fillingPointer = 0;
        private float _sweep = 0.0f;
        private float _step = 0.0f;
        private int _minSweepSamples = 0;
        private int _maxSweepSamples = 0;
        private int _sweepSamples = 0;
        private float _rate = 0.2f;
        private int _delaySamples = 22;

        public WaveFormat WaveFormat => _source.WaveFormat;
        private float sweepRate;

        public bool Enabled { get; set; }

        public float DryWet { get; set; } = 0.5f;

        public float SweepRate
        {
            get
            {
                return sweepRate;
            }
            set
            {
                sweepRate = value;
                _rate = Mathf.Pow(10f, value / _source.WaveFormat.SampleRate);
                _rate -= 1.0f;
                _rate *= 1.1f;
                _rate += 0.1f;
                SetSweep();
            }
        }
        private float width;
        public float Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
                _sweepSamples = Mathf.RoundToInt((float)(value * 0.05 * _source.WaveFormat.SampleRate));
                SetSweep();
            }
        }

        private float delay;
        public float Delay
        {
            get
            {
                return delay;
            }
            set
            {
                delay = value;
                var seconds = Math.Pow(10.0, 2.0) / 1000.0;
                _delaySamples = Mathf.RoundToInt((float)seconds * _source.WaveFormat.SampleRate);
                SetSweep();
            }
        }

        public ChorusModule(ISampleProvider source)
        {
            _source = source;
            SetSweep();
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var samples = _source.Read(buffer, offset, count);

            if (!Enabled)
            {
                return samples;
            }

            if (Width < 0.1f)
            {
                return samples;
            }

            for (var i = 0; i < samples; i++)
            {
                var inputSample = buffer[offset + i];
                _circularBuffer[_fillingPointer] = inputSample;
                _fillingPointer = (_fillingPointer + 1) & (BufferSize - 1);

                var emptyingPointer = (int)(_fillingPointer - _sweep);
                emptyingPointer &= (BufferSize - 1);
                var delayedSample = _circularBuffer[emptyingPointer];

                // Mix wet and dry signals, ensuring amplitude does not exceed +/-1.0...
                buffer[offset + i] = (float)Math.Tanh((1 - DryWet) * inputSample + DryWet * delayedSample);

                // Step the sweep...
                _sweep += _step;

                if (_sweep >= _maxSweepSamples || _sweep <= _minSweepSamples)
                {
                    _step = -_step;
                }
            }

            return samples;
        }

        private void SetSweep()
        {
            // Samples per second needed to achieve desired sweep rate...
            _step = _sweepSamples * 2.0f * _rate / _source.WaveFormat.SampleRate;

            _minSweepSamples = _delaySamples;
            _maxSweepSamples = _delaySamples + _sweepSamples;

            // Set starting _sweep value to mid-range...
            _sweep = (_minSweepSamples + _maxSweepSamples) / 2.0f;
        }
    }
}
