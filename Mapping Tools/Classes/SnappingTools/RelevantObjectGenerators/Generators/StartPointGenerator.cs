﻿using System.Collections.Generic;
using System.Linq;
using Mapping_Tools.Classes.BeatmapHelper;

namespace Mapping_Tools.Classes.SnappingTools.RelevantObjectGenerators.Generators {
    class StartPointGenerator : RelevantObjectsGenerator, IGeneratePointsFromHitObjects {
        public override string Name => "Point on Start of Hit Object Generator";
        public override string Tooltip => "Generates virtual points on slider heads and circles.";
        public override GeneratorType GeneratorType => GeneratorType.Generators;

        public List<RelevantPoint> GetRelevantObjects(List<HitObject> objects)
        {
            return (from ho in objects where !ho.IsSpinner select new RelevantPoint(ho.Pos)).ToList();
        }
    }
}
