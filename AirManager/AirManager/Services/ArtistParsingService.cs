using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AirManager.Services.Database;

namespace AirManager.Services
{
    public static class ArtistParsingService
    {
        private static readonly string[] ArtistSeparators = new[]
        {
            " feat. ", " feat ", " ft. ", " ft ",
            " vs. ", " vs ", " & ", " % ", " e ", ", "
        };

        private static readonly Regex TitleFeatRegex = new Regex(
            @"[\(\[]?\s*(?:feat\.?|ft\.?)\s+(.+?)[\)\]]?\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static (string PrimaryArtist, List<string> FeaturedArtists) ParseArtists(
            string artistField, string titleField, List<ArtistAliasEntry> aliases = null)
        {
            var featured = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string primaryArtist = artistField?.Trim() ?? "";
            bool artistSplitDetected = false;

            foreach (var sep in ArtistSeparators)
            {
                int idx = primaryArtist.IndexOf(sep, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    artistSplitDetected = true;
                    string rest = primaryArtist.Substring(idx + sep.Length);
                    primaryArtist = primaryArtist.Substring(0, idx).Trim();
                    var subSeps = new[] { ", ", " & ", " e ", " % " };
                    var subParts = rest.Split(subSeps, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in subParts)
                    {
                        string trimmed = part.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed))
                            featured.Add(trimmed);
                    }
                    break;
                }
            }

            if (artistSplitDetected && !string.IsNullOrWhiteSpace(primaryArtist))
                featured.Add(primaryArtist);

            if (!string.IsNullOrEmpty(titleField))
            {
                var match = TitleFeatRegex.Match(titleField);
                if (match.Success)
                {
                    string featPart = match.Groups[1].Value.Trim();
                    var subSeps = new[] { ", ", " & ", " e ", " % " };
                    var subParts = featPart.Split(subSeps, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in subParts)
                    {
                        string trimmed = part.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed))
                            featured.Add(trimmed);
                    }
                }
            }

            if (aliases != null && aliases.Count > 0)
            {
                var resolved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var name in featured)
                    resolved.Add(ResolveAlias(name, aliases));
                featured = resolved;
                primaryArtist = ResolveAlias(primaryArtist, aliases);
            }

            if (!artistSplitDetected)
                featured.Remove(primaryArtist);

            return (primaryArtist, featured.ToList());
        }

        public static HashSet<string> GetAllArtists(
            string artist, string featuredArtists, List<ArtistAliasEntry> aliases = null)
        {
            var all = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(artist))
            {
                var parsedArtists = ParseArtists(artist, string.Empty, aliases);
                if (!string.IsNullOrWhiteSpace(parsedArtists.PrimaryArtist))
                    all.Add(parsedArtists.PrimaryArtist);
                foreach (var f in parsedArtists.FeaturedArtists)
                    if (!string.IsNullOrWhiteSpace(f)) all.Add(f);
            }
            if (!string.IsNullOrWhiteSpace(featuredArtists))
            {
                foreach (var fa in featuredArtists.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string name = fa.Trim();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        string resolved = aliases != null ? ResolveAlias(name, aliases) : name;
                        all.Add(resolved);
                    }
                }
            }
            return all;
        }

        public static bool ArtistsOverlap(
            string artistA, string featuredA,
            string artistB, string featuredB,
            List<ArtistAliasEntry> aliases = null)
        {
            var setA = GetAllArtists(artistA, featuredA, aliases);
            var setB = GetAllArtists(artistB, featuredB, aliases);
            return setA.Overlaps(setB);
        }

        public static string ResolveAlias(string name, List<ArtistAliasEntry> aliases)
        {
            if (string.IsNullOrWhiteSpace(name) || aliases == null)
                return name;
            foreach (var entry in aliases)
            {
                if (string.Equals(entry.ArtistName?.Trim(), name, StringComparison.OrdinalIgnoreCase))
                    return entry.ArtistName.Trim();
                if (!string.IsNullOrWhiteSpace(entry.Aliases))
                {
                    var aliasList = entry.Aliases.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    if (aliasList.Any(a => string.Equals(a.Trim(), name, StringComparison.OrdinalIgnoreCase)))
                        return entry.ArtistName?.Trim() ?? name;
                }
            }
            return name;
        }
    }
}
