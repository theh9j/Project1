using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SolverResult {
    Solvable,
    Unsolvable,
    Unknown,
    Invalid
}
public static class LevelProcessor {
    private const int MaxCapacity = 4;

    public static bool CheckForPossibleMoves(List<Bottle> bottles) {
        foreach (Bottle from in bottles) {
            if (from.IsEmpty) return true;
            if (from.Completion || from.isLocked) continue;
            int count = 0;

            LiquidColor current = from.GetTopColor();
            count += from.GetAdjacentColorCount();

            foreach (Bottle to in bottles) {
                if (to.IsEmpty) return true;
                if (to.Completion || to.isLocked || from == to) continue;
                LiquidColor next = to.GetTopColor();
                if (next == current) {
                    count -= (to.maxCapacity - to.liquidUnits.Count);
                    if (count <= 0) break;
                }
            }
            if (count <= 0) return true;
        }
        return false;
    }

    public static string SolveAns(List<Bottle> bottles, int maxSteps = 2000000) { //DEV TOOLS

        return Solve(bottles, maxSteps) switch {
            SolverResult.Solvable => "Solvable",
            SolverResult.Unsolvable => "Unsolvable",
            SolverResult.Unknown => "Unknown",
            SolverResult.Invalid => "Invalid",
            _ => "???"
        };

    }

    private static SolverResult Solve(List<Bottle> bottles, int maxSteps) {

        SolverState start = SolverState.FromBottles(bottles);

        if (!HasValidColorCounts(start))
            return SolverResult.Invalid;

        Stack<SolverState> stack = new();
        HashSet<string> visited = new();

        stack.Push(start);

        int steps = 0;

        while (stack.Count > 0) {
            if (++steps > maxSteps) {
                Debug.LogWarning(
                    $"Solver stopped at {steps} steps. Result unknown."
                );

                return SolverResult.Unknown;
            }

            SolverState state = stack.Pop();
            state.RefreshLocks();

            string key = state.Encode();

            if (!visited.Add(key))
                continue;

            if (state.IsComplete()) {
                Debug.Log($"Solver found solution in {steps} steps.");
                return SolverResult.Solvable;
            }

            foreach (SolverState next in GetNextStates(state)) {
                stack.Push(next);
            }
        }

        return SolverResult.Unsolvable;
    }

    private static IEnumerable<SolverState> GetNextStates(
        SolverState state) {
        for (int from = 0; from < state.Bottles.Count; from++) {
            for (int to = 0; to < state.Bottles.Count; to++) {
                if (from == to)
                    continue;

                if (!CanPour(state, from, to))
                    continue;

                yield return Pour(state, from, to);
            }
        }
    }

    private static bool CanPour(
        SolverState state,
        int from,
        int to) {
        List<LiquidColor> source = state.Bottles[from];
        List<LiquidColor> target = state.Bottles[to];

        if (state.Locked[from]) return false;
        if (state.Locked[to]) return false;

        if (source.Count == 0) return false;
        if (target.Count >= MaxCapacity) return false;

        if (IsCompletedBottle(source)) return false;
        if (IsCompletedBottle(target)) return false;

        LiquidColor movingColor = source[^1];

        if (target.Count == 0)
            return true;

        return target[^1] == movingColor;
    }

    private static SolverState Pour(
        SolverState state,
        int from,
        int to) {
        SolverState next = state.Clone();

        List<LiquidColor> source = next.Bottles[from];
        List<LiquidColor> target = next.Bottles[to];

        LiquidColor color = source[^1];

        while (
            source.Count > 0 &&
            target.Count < MaxCapacity &&
            source[^1] == color) {
            target.Add(source[^1]);
            source.RemoveAt(source.Count - 1);
        }

        next.RefreshLocks();

        return next;
    }

    private static bool HasValidColorCounts(SolverState state) {
        Dictionary<LiquidColor, int> counts = new();

        foreach (List<LiquidColor> bottle in state.Bottles) {
            foreach (LiquidColor color in bottle) {
                counts.TryAdd(color, 0);
                counts[color]++;
            }
        }

        foreach (var pair in counts) {
            if (pair.Value != MaxCapacity) {
                Debug.LogError(
                    $"Invalid level: {pair.Key} appears {pair.Value} times."
                );

                return false;
            }
        }

        return true;
    }

    private static bool IsCompletedBottle(List<LiquidColor> bottle) {
        if (bottle.Count != MaxCapacity)
            return false;

        LiquidColor color = bottle[0];

        for (int i = 1; i < bottle.Count; i++) {
            if (bottle[i] != color)
                return false;
        }

        return true;
    }

    private class SolverState {
        public List<List<LiquidColor>> Bottles;
        public bool[] Locked;
        public LiquidColor[] LockConditions;

        public static SolverState FromBottles(List<Bottle> bottles) {
            SolverState state = new SolverState();

            state.Bottles = bottles
                .Select(b => b.liquidUnits
                    .Select(l => l.colorId)
                    .ToList())
                .ToList();

            state.Locked = bottles
                .Select(b => b.isLocked)
                .ToArray();

            state.LockConditions = bottles
                .Select(b => b.lockColor)
                .ToArray();

            state.RefreshLocks();

            return state;
        }

        public SolverState Clone() {
            return new SolverState {
                Bottles = Bottles
                    .Select(b => new List<LiquidColor>(b))
                    .ToList(),

                Locked = (bool[])Locked.Clone(),
                LockConditions = (LiquidColor[])LockConditions.Clone()
            };
        }

        public void RefreshLocks() {
            HashSet<LiquidColor> completedColors = new();

            foreach (List<LiquidColor> bottle in Bottles) {
                if (IsCompletedBottle(bottle))
                    completedColors.Add(bottle[0]);
            }

            for (int i = 0; i < Locked.Length; i++) {
                if (!Locked[i])
                    continue;

                if (completedColors.Contains(LockConditions[i]))
                    Locked[i] = false;
            }
        }

        public bool IsComplete() {
            foreach (List<LiquidColor> bottle in Bottles) {
                if (bottle.Count == 0)
                    continue;

                if (!IsCompletedBottle(bottle))
                    return false;
            }

            return true;
        }

        public string Encode() {
            return string.Join("|",
                Bottles.Select((bottle, i) =>
                    $"{(Locked[i] ? 1 : 0)}:" +
                    $"{(int)LockConditions[i]}:" +
                    string.Join(",", bottle.Select(c => (int)c))
                )
            );
        }
    }
}