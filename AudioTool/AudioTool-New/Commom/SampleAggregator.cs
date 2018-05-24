using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NAudio.Dsp;
using NAudio.Wave;

namespace AudioToolNew
{
    public class SampleAggregator : ISampleProvider
    {
        // volume
        public event EventHandler<MaxSampleEventArgs> MaximumCalculated;
        private float _maxValue;
        private float _minValue;
        public int NotificationCount { get; set; }
        int _count;

        private readonly ISampleProvider _source;
        private readonly int _channels;

        public SampleAggregator(ISampleProvider source)
        {
            _channels = source.WaveFormat.Channels;
            this._source = source;
        }

        public void Reset()
        {
            _count = 0;
            _maxValue = _minValue = 0;
        }

        public WaveFormat WaveFormat
        {
            get { return _source.WaveFormat; }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = _source.Read(buffer, offset, count);

            for (int n = 0; n < samplesRead; n += _channels)
            {
                Add(buffer[n + offset]);
            }
            return samplesRead;
        }

        private void Add(float value)
        {
            _maxValue = Math.Max(_maxValue, value);
            _minValue = Math.Min(_minValue, value);
            _count++;
            if (_count >= NotificationCount && NotificationCount > 0)
            {
                if (MaximumCalculated != null) MaximumCalculated.Invoke(this, new MaxSampleEventArgs(_minValue, _maxValue));

                Reset();
            }
        }
    }
    public class MaxSampleEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public MaxSampleEventArgs(float minValue, float maxValue)
        {
            MaxSample = maxValue;
            MinSample = minValue;
        }
        public float MaxSample { get; private set; }
        public float MinSample { get; private set; }
    }
}
