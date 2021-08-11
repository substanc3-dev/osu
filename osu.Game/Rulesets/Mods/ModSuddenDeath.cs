// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModSuddenDeath : ModFailCondition
    {
        public override string Name => "Sudden Death";
        public override string Acronym => "SD";
        public override IconUsage? Icon => OsuIcon.ModSuddendeath;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => "Miss and fail.";
        public override double ScoreMultiplier => 1;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModPerfect)).ToArray();

        [SettingSource("Restart on fail", "Automatically restarts when failed.")]
        public BindableBool Restart { get; } = new BindableBool();

        public override bool PerformFail() => true;

        public override bool RestartOnFail => Restart.Value;

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
            => result.Type.AffectsCombo()
               && !result.IsHit;
    }
}
