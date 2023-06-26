﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable
// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Filter;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select
{
    public class FilterCriteria
    {
        public GroupMode Group;
        public SortMode Sort;

        public BeatmapSetInfo? SelectedBeatmapSet;

        public OptionalRange<double> StarDifficulty;
        public OptionalRange<float> ApproachRate;
        public OptionalRange<float> DrainRate;
        public OptionalRange<float> CircleSize;
        public OptionalRange<float> OverallDifficulty;
        public OptionalRange<double> Length;
        public OptionalRange<double> BPM;
        public OptionalRange<int> BeatDivisor;
        public OptionalRange<BeatmapOnlineStatus> OnlineStatus;
        public OptionalTextFilter Creator;
        public OptionalTextFilter Artist;

        public OptionalRange<double> UserStarDifficulty = new OptionalRange<double>
        {
            IsLowerInclusive = true,
            IsUpperInclusive = true
        };

        public OptionalTextFilter[] SearchTerms = Array.Empty<OptionalTextFilter>();

        public RulesetInfo? Ruleset;
        public bool AllowConvertedBeatmaps;

        private string searchText = string.Empty;

        /// <summary>
        /// <see cref="SearchText"/> as a number (if it can be parsed as one).
        /// </summary>
        public int? SearchNumber { get; private set; }

        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;

                List<OptionalTextFilter> terms = new List<OptionalTextFilter>();

                string remainingText = value;

                // First handle quoted segments to ensure we keep inline spaces in exact matches.
                foreach (Match quotedSegment in Regex.Matches(searchText, "(\"[^\"]+\")"))
                {
                    terms.Add(new OptionalTextFilter { SearchTerm = quotedSegment.Value });
                    remainingText = remainingText.Replace(quotedSegment.Value, string.Empty);
                }

                // Then handle the rest splitting on any spaces.
                terms.AddRange(remainingText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => new OptionalTextFilter
                {
                    SearchTerm = s
                }));

                SearchTerms = terms.ToArray();

                SearchNumber = null;

                if (SearchTerms.Length == 1 && int.TryParse(SearchTerms[0].SearchTerm, out int parsed))
                    SearchNumber = parsed;
            }
        }

        /// <summary>
        /// Hashes from the <see cref="BeatmapCollection"/> to filter to.
        /// </summary>
        public IEnumerable<string>? CollectionBeatmapMD5Hashes { get; set; }

        public IRulesetFilterCriteria? RulesetCriteria { get; set; }

        public struct OptionalRange<T> : IEquatable<OptionalRange<T>>
            where T : struct
        {
            public bool HasFilter => Max != null || Min != null;

            public bool IsInRange(T value)
            {
                if (Min != null)
                {
                    int comparison = Comparer<T>.Default.Compare(value, Min.Value);

                    if (comparison < 0)
                        return false;

                    if (comparison == 0 && !IsLowerInclusive)
                        return false;
                }

                if (Max != null)
                {
                    int comparison = Comparer<T>.Default.Compare(value, Max.Value);

                    if (comparison > 0)
                        return false;

                    if (comparison == 0 && !IsUpperInclusive)
                        return false;
                }

                return true;
            }

            public T? Min;
            public T? Max;
            public bool IsLowerInclusive;
            public bool IsUpperInclusive;

            public bool Equals(OptionalRange<T> other)
                => EqualityComparer<T?>.Default.Equals(Min, other.Min)
                   && EqualityComparer<T?>.Default.Equals(Max, other.Max)
                   && IsLowerInclusive.Equals(other.IsLowerInclusive)
                   && IsUpperInclusive.Equals(other.IsUpperInclusive);
        }

        public struct OptionalTextFilter : IEquatable<OptionalTextFilter>
        {
            public bool HasFilter => !string.IsNullOrEmpty(SearchTerm);

            public bool Exact { get; private set; }

            public bool Matches(string value)
            {
                if (!HasFilter)
                    return true;

                // search term is guaranteed to be non-empty, so if the string we're comparing is empty, it's not matching
                if (string.IsNullOrEmpty(value))
                    return false;

                if (Exact)
                    return Regex.IsMatch(value, $@"(^|\s){searchTerm}($|\s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                return value.Contains(SearchTerm, StringComparison.InvariantCultureIgnoreCase);
            }

            private string searchTerm;

            public string SearchTerm
            {
                get => searchTerm;
                set
                {
                    searchTerm = value.Trim('"');
                    Exact = searchTerm != value;
                }
            }

            public bool Equals(OptionalTextFilter other) => SearchTerm == other.SearchTerm;
        }
    }
}
