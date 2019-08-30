﻿using Mapping_Tools.Classes.HitsoundStuff;
using Mapping_Tools.Classes.MathUtil;
using System;
using System.Collections;
using static Mapping_Tools.Classes.BeatmapHelper.FileFormatHelper;

namespace Mapping_Tools.Classes.BeatmapHelper {
    public class TimingPoint : ITextLine {
        // Offset, Milliseconds per Beat, Meter, Sample Set, Sample Index, Volume, Inherited, Kiai Mode
        public double Offset { get; set; }
        public double MpB { get; set; }
        public int Meter { get; set; }
        public SampleSet SampleSet { get; set; }
        public int SampleIndex { get; set; }
        public double Volume { get; set; }
        public bool Inherited { get; set; } // True is red line
        public bool Kiai { get; set; }
        public bool OmitFirstBarLine { get; set; }

        public TimingPoint(double offset, double mpb, int meter, SampleSet sampleSet, int sampleIndex, double volume, bool inherited, bool kiai, bool omitFirstBarLine) {
            Offset = offset;
            MpB = mpb;
            Meter = meter;
            SampleSet = sampleSet;
            SampleIndex = sampleIndex;
            Volume = volume;
            Inherited = inherited;
            Kiai = kiai;
            OmitFirstBarLine = omitFirstBarLine;
        }

        public TimingPoint(Editor_Reader.ControlPoint cp) {
            MpB = cp.BeatLength;
            Offset = cp.Offset;
            SampleIndex = cp.CustomSamples;
            SampleSet = (SampleSet)cp.SampleSet;
            Meter = cp.TimeSignature;
            Volume = cp.Volume;
            Kiai = (cp.EffectFlags & 1) > 0;
            OmitFirstBarLine = (cp.EffectFlags & 8) > 0;
            Inherited = cp.TimingChange;
        }

        public static explicit operator TimingPoint(Editor_Reader.ControlPoint cp) {
            return new TimingPoint(cp);
        }

        public TimingPoint(string line) {
            SetLine(line);
        }

        public string GetLine() {
            int style = MathHelper.GetIntFromBitArray(new BitArray(new bool[] { Kiai, false, false, OmitFirstBarLine }));
            return $"{Offset.ToRoundInvariant()},{MpB.ToInvariant()},{Meter.ToInvariant()},{SampleSet.ToIntInvariant()},{SampleIndex.ToInvariant()},{Volume.ToRoundInvariant()},{Convert.ToInt32(Inherited).ToInvariant()},{style.ToInvariant()}";
        }

        public void SetLine(string line) {
            string[] values = line.Split(',');

            if (TryParseDouble(values[0], out double offset))
                Offset = offset;
            else throw new BeatmapParsingException("Failed to parse offset of timing point", line);

            if (TryParseDouble(values[1], out double mpb))
                MpB = mpb;
            else throw new BeatmapParsingException("Failed to parse milliseconds per beat of timing point", line);

            if (TryParseInt(values[2], out int meter))
                Meter = meter;
            else throw new BeatmapParsingException("Failed to parse meter of timing point", line);

            if (Enum.TryParse(values[3], out SampleSet ss))
                SampleSet = ss;
            else throw new BeatmapParsingException("Failed to parse sampleset of timing point", line);

            if (TryParseInt(values[4], out int ind))
                SampleIndex = ind;
            else throw new BeatmapParsingException("Failed to parse samle index of timing point", line);

            if (TryParseDouble(values[5], out double vol))
                Volume = vol;
            else throw new BeatmapParsingException("Failed to parse volume of timing point", line);

            Inherited = values[6] == "1";

            if (TryParseInt(values[7], out int style)) {
                BitArray b = new BitArray(new int[] { style });
                Kiai = b[0];
                OmitFirstBarLine = b[3];
            } else throw new BeatmapParsingException("Failed to style of timing point", line);
        }

        public TimingPoint Copy() {
            return new TimingPoint(Offset, MpB, Meter, SampleSet, SampleIndex, Volume, Inherited, Kiai, OmitFirstBarLine);
        }

        public bool ResnapSelf(Timing timing, int snap1, int snap2, bool floor=true, TimingPoint tp=null, TimingPoint firstTP = null) {
            double newTime = timing.Resnap(Offset, snap1, snap2, floor, tp, firstTP);
            double deltaTime = newTime - Offset;
            Offset += deltaTime;
            return deltaTime != 0;
        }

        public bool Equals(TimingPoint tp) {
            return Offset == tp.Offset &&
                MpB == tp.MpB &&
                Meter == tp.Meter &&
                SampleSet == tp.SampleSet &&
                SampleIndex == tp.SampleIndex &&
                Volume == tp.Volume &&
                Inherited == tp.Inherited &&
                Kiai == tp.Kiai &&
                OmitFirstBarLine == OmitFirstBarLine;
        }

        public bool SameEffect(TimingPoint tp) {
            if (tp.Inherited && !Inherited) {
                return MpB == -100 && Meter == tp.Meter && SampleSet == tp.SampleSet && SampleIndex == tp.SampleIndex && Volume == tp.Volume && Kiai == tp.Kiai;
            }
            return MpB == tp.MpB && Meter == tp.Meter && SampleSet == tp.SampleSet && SampleIndex == tp.SampleIndex && Volume == tp.Volume && Kiai == tp.Kiai;
        }

        public double GetBPM() {
            if( Inherited ) {
                return 60000 / MpB;
            }
            else {
                return -100 / MpB;
            }
        }
    }
}
