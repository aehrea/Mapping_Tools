﻿using System;
using JetBrains.Annotations;
using Mapping_Tools_Core.Audio.SampleGeneration;
using Mapping_Tools_Core.BeatmapHelper.Enums;

namespace Mapping_Tools_Core.Tools.HitsoundStudio.Model {
    /// <summary>
    /// Represents a hitsound sample in osu!
    /// with the addition of an outside volume parameter.
    /// </summary>
    public interface ISample : ICloneable {
        [CanBeNull]
        ISampleGenerator SampleGenerator { get; set; }
        int Priority { get; set; }
        double OutsideVolume { get; set; }
        SampleSet SampleSet { get; set; }
        Hitsound Hitsound { get; set; }
    }
}