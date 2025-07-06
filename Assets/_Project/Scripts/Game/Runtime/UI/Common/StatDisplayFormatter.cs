using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game
{
    public static class StatDisplayFormatter
    {
        private const string HighlightColor = "#FFE066";
        private const string TransitionSymbol = " \u2014 ";

        public static void AppendSpellStatRow(
            StringBuilder namesBuilder,
            StringBuilder valuesBuilder,
            string statName,
            List<float> valuesPerLevel,
            int currentTier)
        {
            namesBuilder.Append(statName).Append(":\n");

            if (valuesPerLevel == null || valuesPerLevel.Count == 0)
            {
                valuesBuilder.Append("0\n");
                return;
            }

            var isTime = IsTimeStat(statName);

            if (currentTier < 0)
            {
                var initialValue = valuesPerLevel.ValueAtLevel(0);
                valuesBuilder.Append(FormatSpellValue(initialValue, isTime)).Append('\n');
                return;
            }

            var currentValue = valuesPerLevel.ValueAtLevel(currentTier);
            var nextValue = valuesPerLevel.ValueAtLevel(currentTier + 1);

            if (Mathf.Approximately(currentValue, nextValue))
            {
                valuesBuilder.Append(FormatSpellValue(currentValue, isTime)).Append('\n');
            }
            else
            {
                valuesBuilder.Append(FormatSpellValue(currentValue, isTime))
                    .Append(TransitionSymbol)
                    .Append("<color=")
                    .Append(HighlightColor)
                    .Append('>')
                    .Append(FormatSpellValue(nextValue, isTime))
                    .Append("</color>\n");
            }
        }

        public static void AppendItemModifierRow(
            StringBuilder namesBuilder,
            StringBuilder valuesBuilder,
            StatType stat,
            ModifierOp op,
            List<float> valuesPerLevel,
            int currentTier)
        {
            namesBuilder.Append(stat.ToString()).Append(":\n");

            if (valuesPerLevel == null || valuesPerLevel.Count == 0)
            {
                valuesBuilder.Append("0\n");
                return;
            }

            if (currentTier < 0)
            {
                var initialValue = valuesPerLevel.ValueAtLevel(0);
                valuesBuilder.Append(FormatModifierValue(initialValue, op, stat)).Append('\n');
                return;
            }

            var currentValue = valuesPerLevel.ValueAtLevel(currentTier);
            var nextValue = valuesPerLevel.ValueAtLevel(currentTier + 1);

            if (Mathf.Approximately(currentValue, nextValue))
            {
                valuesBuilder.Append(FormatModifierValue(currentValue, op, stat)).Append('\n');
            }
            else
            {
                valuesBuilder.Append(FormatModifierValue(currentValue, op, stat))
                    .Append(TransitionSymbol)
                    .Append("<color=")
                    .Append(HighlightColor)
                    .Append('>')
                    .Append(FormatModifierValue(nextValue, op, stat))
                    .Append("</color>\n");
            }
        }

        private static bool IsTimeStat(string statName)
        {
            var isTime = statName.Equals("Cooldown", StringComparison.OrdinalIgnoreCase)
                || statName.Equals("Duration", StringComparison.OrdinalIgnoreCase)
                || statName.Equals("Delay", StringComparison.OrdinalIgnoreCase);
            return isTime;
        }

        private static string FormatSpellValue(float value, bool isTime)
        {
            var str = value.ToString("0.##");
            var result = isTime ? $"{str}s" : str;
            return result;
        }

        private static string FormatModifierValue(float value, ModifierOp op, StatType stat)
        {
            if (op == ModifierOp.AdditivePercent)
            {
                var percent = value * 100f;
                var sign = percent > 0 ? "+" : "";
                var formattedPercent = $"{sign}{percent:0.##}%";
                return formattedPercent;
            }

            var flatSign = value > 0 ? "+" : "";
            var suffix = stat == StatType.Cooldown ? "s" : "";
            var formattedFlat = $"{flatSign}{value:0.##}{suffix}";
            return formattedFlat;
        }
    }
}
