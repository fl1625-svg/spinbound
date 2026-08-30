using System;
using System.Collections.Generic;
using System.Numerics;
using Spinbound.Core.Collision;
using Spinbound.Core.Reference;
using Spinbound.Core.Simulation;

namespace Spinbound.Worlds.W01.DaisyMeadow
{
    public sealed class W01StageRouteContract
    {
        public W01StageRouteContract(
            StageDefinition stage,
            IReadOnlyList<ReferenceAction> safe,
            IReadOnlyList<ReferenceAction> skilled)
        {
            Stage = stage ?? throw new ArgumentNullException(nameof(stage));
            Safe = safe ?? throw new ArgumentNullException(nameof(safe));
            Skilled = skilled ?? throw new ArgumentNullException(nameof(skilled));
        }

        public StageDefinition Stage { get; }
        public IReadOnlyList<ReferenceAction> Safe { get; }
        public IReadOnlyList<ReferenceAction> Skilled { get; }
    }

    internal static class W01RouteAuthoring
    {
        public static RotorState Start(float x, float y) => new(
            new Vector2(x, y),
            0f,
            -RotorTuning.BaseAngularSpeedDegPerSecond,
            RotationDirection.Clockwise,
            RotorMode.Standard,
            Vector2.Zero);

        public static ReferenceAction Move(float x, float y, SpeedTier tier, float maxSeconds) =>
            ReferenceAction.MoveTo(new Vector2(x, y), tier, maxSeconds);

        public static ReferenceAction Wait(float seconds) => ReferenceAction.Wait(seconds);

        public static IReadOnlyList<ReferenceAction> Route(params ReferenceAction[] actions) => actions;

        public static IReadOnlyList<CourseCollider> Geometry(
            string stageId,
            float minX,
            float maxX,
            float minY,
            float maxY,
            params CourseCollider[] obstacles)
        {
            const float rim = 0.7f;
            var result = new CourseCollider[4 + obstacles.Length];
            result[0] = new CourseCollider(stageId + "-north-rim", new Vector2(minX, maxY), new Vector2(maxX, maxY + rim));
            result[1] = new CourseCollider(stageId + "-south-rim", new Vector2(minX, minY - rim), new Vector2(maxX, minY));
            result[2] = new CourseCollider(stageId + "-west-rim", new Vector2(minX - rim, minY - rim), new Vector2(minX, maxY + rim));
            result[3] = new CourseCollider(stageId + "-east-rim", new Vector2(maxX, minY - rim), new Vector2(maxX + rim, maxY + rim));
            Array.Copy(obstacles, 0, result, 4, obstacles.Length);
            return result;
        }
    }

    public static class W01_01_FirstSpin
    {
        public const string Id = "W01-01";

        public static readonly StageDefinition Definition = new(
            Id,
            "First Spin",
            "Teach the first clean 90-degree route read with a generous garden pocket.",
            StageKind.Normal,
            W01RouteAuthoring.Start(-10f, 0f),
            new Vector2(10f, 0f),
            0.7f,
            W01RouteAuthoring.Geometry(
                Id, -12f, 12f, -6.5f, 6.5f,
                new CourseCollider("W01-01-garden-mass", new Vector2(-2.6f, -1.2f), new Vector2(-1.0f, 1.2f)),
                new CourseCollider("W01-01-upper-grove", new Vector2(6.6f, 1.4f), new Vector2(8.4f, 4.8f))),
            8.47f);

        public static readonly IReadOnlyList<ReferenceAction> Safe = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-5f, 0f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Move(-5f, 4f, SpeedTier.Speed1, 3f),
            W01RouteAuthoring.Move(0f, 4f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Move(0f, -3f, SpeedTier.Speed1, 4f),
            W01RouteAuthoring.Move(5f, -3f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Move(5f, 0f, SpeedTier.Speed1, 2f),
            W01RouteAuthoring.Move(10f, 0f, SpeedTier.Speed2, 3f));

        public static readonly IReadOnlyList<ReferenceAction> Skilled = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-5f, 0f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(-5f, 4f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(0f, 4f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(0f, -3f, SpeedTier.Speed3, 4f),
            W01RouteAuthoring.Move(5f, -3f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(5f, 0f, SpeedTier.Speed3, 2f),
            W01RouteAuthoring.Move(10f, 0f, SpeedTier.Speed3, 3f));
    }

    public static class W01_02_BloomingGates
    {
        public const string Id = "W01-02";

        public static readonly StageDefinition Definition = new(
            Id,
            "Blooming Gates",
            "Layer timing beats onto a readable straight-line garden sprint.",
            StageKind.Normal,
            W01RouteAuthoring.Start(-11f, 0f),
            new Vector2(11f, 0f),
            0.7f,
            W01RouteAuthoring.Geometry(
                Id, -13f, 13f, -6f, 6f,
                new CourseCollider("W01-02-gate-a-north", new Vector2(-6.45f, 2.15f), new Vector2(-5.55f, 6f)),
                new CourseCollider("W01-02-gate-a-south", new Vector2(-6.45f, -6f), new Vector2(-5.55f, -2.15f)),
                new CourseCollider("W01-02-gate-b-north", new Vector2(-1.45f, 2.15f), new Vector2(-0.55f, 6f)),
                new CourseCollider("W01-02-gate-b-south", new Vector2(-1.45f, -6f), new Vector2(-0.55f, -2.15f)),
                new CourseCollider("W01-02-gate-c-north", new Vector2(3.55f, 2.15f), new Vector2(4.45f, 6f)),
                new CourseCollider("W01-02-gate-c-south", new Vector2(3.55f, -6f), new Vector2(4.45f, -2.15f))),
            6.50f);

        public static readonly IReadOnlyList<ReferenceAction> Safe = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-6f, 0f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Wait(0.75f),
            W01RouteAuthoring.Move(-1f, 0f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Wait(0.5f),
            W01RouteAuthoring.Move(4f, 0f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Wait(0.75f),
            W01RouteAuthoring.Move(11f, 0f, SpeedTier.Speed2, 3f));

        public static readonly IReadOnlyList<ReferenceAction> Skilled = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-6f, 0f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Wait(0.25f),
            W01RouteAuthoring.Move(-1f, 0f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Wait(0.25f),
            W01RouteAuthoring.Move(4f, 0f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Wait(0.25f),
            W01RouteAuthoring.Move(11f, 0f, SpeedTier.Speed3, 3f));
    }

    public static class W01_03_GardenSwitchback
    {
        public const string Id = "W01-03";

        public static readonly StageDefinition Definition = new(
            Id,
            "Garden Switchback",
            "Build route-planning confidence through a long, readable zig-zag.",
            StageKind.Normal,
            W01RouteAuthoring.Start(-10f, -4f),
            new Vector2(10f, 0f),
            0.7f,
            W01RouteAuthoring.Geometry(
                Id, -12f, 12f, -6.5f, 6.5f,
                new CourseCollider("W01-03-center-hedge", new Vector2(-0.5f, 0.35f), new Vector2(0.5f, 1.45f)),
                new CourseCollider("W01-03-east-grove", new Vector2(8.3f, 2.0f), new Vector2(10.0f, 4.8f))),
            10.30f);

        public static readonly IReadOnlyList<ReferenceAction> Safe = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-6f, -4f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Move(-6f, 3f, SpeedTier.Speed1, 4f),
            W01RouteAuthoring.Move(-2f, 3f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Move(-2f, -2f, SpeedTier.Speed1, 3f),
            W01RouteAuthoring.Move(2f, -2f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Move(2f, 4f, SpeedTier.Speed1, 4f),
            W01RouteAuthoring.Move(6f, 4f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Move(6f, 0f, SpeedTier.Speed1, 3f),
            W01RouteAuthoring.Move(10f, 0f, SpeedTier.Speed2, 3f));

        public static readonly IReadOnlyList<ReferenceAction> Skilled = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-6f, -4f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(-6f, 3f, SpeedTier.Speed3, 4f),
            W01RouteAuthoring.Move(-2f, 3f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(-2f, -2f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(2f, -2f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(2f, 4f, SpeedTier.Speed3, 4f),
            W01RouteAuthoring.Move(6f, 4f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(6f, 0f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(10f, 0f, SpeedTier.Speed3, 3f));
    }

    public static class W01_04_WindmillWalk
    {
        public const string Id = "W01-04";

        public static readonly StageDefinition Definition = new(
            Id,
            "Windmill Walk",
            "Teach pause-read-go rhythm around large moving-mechanism landmarks.",
            StageKind.Normal,
            W01RouteAuthoring.Start(-10f, 0f),
            new Vector2(10f, 0f),
            0.7f,
            W01RouteAuthoring.Geometry(
                Id, -12f, 12f, -6.5f, 6.5f,
                new CourseCollider("W01-04-mill-a-base", new Vector2(-5.3f, -5.6f), new Vector2(-3.5f, -4.1f)),
                new CourseCollider("W01-04-mill-b-base", new Vector2(1.9f, 4.8f), new Vector2(3.7f, 6.0f)),
                new CourseCollider("W01-04-mill-c-base", new Vector2(5.0f, -6.0f), new Vector2(6.8f, -4.8f))),
            7.02f);

        public static readonly IReadOnlyList<ReferenceAction> Safe = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-6f, 0f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Wait(0.5f),
            W01RouteAuthoring.Move(-3f, 3f, SpeedTier.Speed1, 3f),
            W01RouteAuthoring.Move(1f, 3f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Wait(0.5f),
            W01RouteAuthoring.Move(4f, 0f, SpeedTier.Speed1, 3f),
            W01RouteAuthoring.Move(7f, -3f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Wait(0.5f),
            W01RouteAuthoring.Move(10f, 0f, SpeedTier.Speed2, 3f));

        public static readonly IReadOnlyList<ReferenceAction> Skilled = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-6f, 0f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Wait(0.2f),
            W01RouteAuthoring.Move(-3f, 3f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(1f, 3f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Wait(0.2f),
            W01RouteAuthoring.Move(4f, 0f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(7f, -3f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Wait(0.2f),
            W01RouteAuthoring.Move(10f, 0f, SpeedTier.Speed3, 3f));
    }

    public static class W01_05_HiddenHedgeway
    {
        public const string Id = "W01-05";

        public static readonly StageDefinition Definition = new(
            Id,
            "Hidden Hedgeway",
            "Offer a readable safe lane and a faster lower hedge shortcut.",
            StageKind.Normal,
            W01RouteAuthoring.Start(-10f, 0f),
            new Vector2(10f, 0f),
            0.7f,
            W01RouteAuthoring.Geometry(
                Id, -12f, 12f, -6.5f, 6.5f,
                new CourseCollider("W01-05-main-hedge", new Vector2(-3.5f, 0.6f), new Vector2(3.5f, 2.0f)),
                new CourseCollider("W01-05-north-hedge", new Vector2(-2f, 2.3f), new Vector2(3.5f, 2.5f))),
            5.86f);

        public static readonly IReadOnlyList<ReferenceAction> Safe = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-6f, 0f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Move(-6f, 4f, SpeedTier.Speed1, 3f),
            W01RouteAuthoring.Move(5.5f, 4f, SpeedTier.Speed2, 5f),
            W01RouteAuthoring.Move(5.5f, -4f, SpeedTier.Speed1, 5f),
            W01RouteAuthoring.Move(8f, -4f, SpeedTier.Speed2, 2f),
            W01RouteAuthoring.Move(8f, 0f, SpeedTier.Speed1, 3f),
            W01RouteAuthoring.Move(10f, 0f, SpeedTier.Speed2, 2f));

        public static readonly IReadOnlyList<ReferenceAction> Skilled = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-6f, 0f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(-3f, -3f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(3f, -3f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(6f, 0f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(10f, 0f, SpeedTier.Speed3, 3f));
    }

    public static class W01_06_FestivalRun
    {
        public const string Id = "W01-06";

        public static readonly StageDefinition Definition = new(
            Id,
            "Festival Run",
            "World 1 graduation run combining corners, pacing windows, and route memory.",
            StageKind.Normal,
            W01RouteAuthoring.Start(-11f, -3f),
            new Vector2(11f, 0f),
            0.7f,
            W01RouteAuthoring.Geometry(
                Id, -13f, 13f, -6.5f, 6.5f,
                new CourseCollider("W01-06-festival-a", new Vector2(-5.2f, -1.4f), new Vector2(-3.6f, 0.1f)),
                new CourseCollider("W01-06-festival-b", new Vector2(0f, 0.4f), new Vector2(1.2f, 1.5f)),
                new CourseCollider("W01-06-festival-c", new Vector2(4.7f, -1.1f), new Vector2(5.5f, 1.1f))),
            10.47f);

        public static readonly IReadOnlyList<ReferenceAction> Safe = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-7f, -3f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Move(-7f, 2f, SpeedTier.Speed1, 3f),
            W01RouteAuthoring.Wait(0.5f),
            W01RouteAuthoring.Move(-2f, 2f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Move(-2f, -3f, SpeedTier.Speed1, 3f),
            W01RouteAuthoring.Move(3f, -3f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Wait(0.5f),
            W01RouteAuthoring.Move(3f, 3f, SpeedTier.Speed1, 4f),
            W01RouteAuthoring.Move(7f, 3f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Move(7f, 0f, SpeedTier.Speed1, 3f),
            W01RouteAuthoring.Move(11f, 0f, SpeedTier.Speed2, 3f));

        public static readonly IReadOnlyList<ReferenceAction> Skilled = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-7f, -3f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(-7f, 2f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Wait(0.2f),
            W01RouteAuthoring.Move(-2f, 2f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(-2f, -3f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(3f, -3f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Wait(0.2f),
            W01RouteAuthoring.Move(3f, 3f, SpeedTier.Speed3, 4f),
            W01RouteAuthoring.Move(7f, 3f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(7f, 0f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(11f, 0f, SpeedTier.Speed3, 3f));
    }

    public static class W01_Trial_PerfectCorner
    {
        public const string Id = "W01-TRIAL-01";

        public static readonly StageDefinition Definition = new(
            Id,
            "Perfect Corner",
            "A short precision exam built around four clean ninety-degree placements.",
            StageKind.Trial,
            W01RouteAuthoring.Start(-5f, -5f),
            new Vector2(5f, 5f),
            0.7f,
            W01RouteAuthoring.Geometry(
                Id, -7f, 7f, -7f, 7f,
                new CourseCollider("W01-TRIAL-01-corner-a", new Vector2(-3.2f, -5.5f), new Vector2(-1.8f, -1.8f)),
                new CourseCollider("W01-TRIAL-01-corner-b", new Vector2(1.8f, -1.8f), new Vector2(3.2f, 3.2f))),
            6.75f);

        public static readonly IReadOnlyList<ReferenceAction> Safe = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-5f, 0f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Move(0f, 0f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Move(0f, 5f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Move(5f, 5f, SpeedTier.Speed2, 3f));

        public static readonly IReadOnlyList<ReferenceAction> Skilled = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-5f, 0f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(0f, 0f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(0f, 5f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(5f, 5f, SpeedTier.Speed3, 3f));
    }

    public static class W01_Boss_BloomEngine
    {
        public const string Id = "W01-BOSS";

        public static readonly StageDefinition Definition = new(
            Id,
            "Bloom Engine",
            "Navigate three deterministic machine phases that remix World 1's learned skills.",
            StageKind.Boss,
            W01RouteAuthoring.Start(-10f, 0f),
            new Vector2(10f, 0f),
            0.8f,
            W01RouteAuthoring.Geometry(
                Id, -12f, 12f, -6.5f, 6.5f,
                new CourseCollider("W01-BOSS-core", new Vector2(-1.5f, -1.5f), new Vector2(1.5f, 1.5f)),
                new CourseCollider("W01-BOSS-west-piston", new Vector2(-7.8f, -5.4f), new Vector2(-6.2f, -3.8f)),
                new CourseCollider("W01-BOSS-east-piston", new Vector2(6.2f, 3.8f), new Vector2(7.8f, 5.4f))),
            0f,
            3);

        public static readonly IReadOnlyList<ReferenceAction> Safe = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-6f, 0f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Wait(0.75f),
            W01RouteAuthoring.Move(-3f, 4f, SpeedTier.Speed1, 4f),
            W01RouteAuthoring.Move(4f, 4f, SpeedTier.Speed2, 4f),
            W01RouteAuthoring.Wait(0.75f),
            W01RouteAuthoring.Move(4f, -4f, SpeedTier.Speed1, 5f),
            W01RouteAuthoring.Move(7f, -4f, SpeedTier.Speed2, 3f),
            W01RouteAuthoring.Wait(0.75f),
            W01RouteAuthoring.Move(10f, 0f, SpeedTier.Speed2, 3f));

        public static readonly IReadOnlyList<ReferenceAction> Skilled = W01RouteAuthoring.Route(
            W01RouteAuthoring.Move(-6f, 0f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Wait(0.35f),
            W01RouteAuthoring.Move(-3f, 4f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Move(4f, 4f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Wait(0.35f),
            W01RouteAuthoring.Move(4f, -4f, SpeedTier.Speed3, 4f),
            W01RouteAuthoring.Move(7f, -4f, SpeedTier.Speed3, 3f),
            W01RouteAuthoring.Wait(0.35f),
            W01RouteAuthoring.Move(10f, 0f, SpeedTier.Speed3, 3f));
    }

    public static class W01ReferenceRoutes
    {
        private static readonly W01StageRouteContract[] Contracts =
        {
            new(W01_01_FirstSpin.Definition, W01_01_FirstSpin.Safe, W01_01_FirstSpin.Skilled),
            new(W01_02_BloomingGates.Definition, W01_02_BloomingGates.Safe, W01_02_BloomingGates.Skilled),
            new(W01_03_GardenSwitchback.Definition, W01_03_GardenSwitchback.Safe, W01_03_GardenSwitchback.Skilled),
            new(W01_04_WindmillWalk.Definition, W01_04_WindmillWalk.Safe, W01_04_WindmillWalk.Skilled),
            new(W01_05_HiddenHedgeway.Definition, W01_05_HiddenHedgeway.Safe, W01_05_HiddenHedgeway.Skilled),
            new(W01_06_FestivalRun.Definition, W01_06_FestivalRun.Safe, W01_06_FestivalRun.Skilled),
            new(W01_Trial_PerfectCorner.Definition, W01_Trial_PerfectCorner.Safe, W01_Trial_PerfectCorner.Skilled),
            new(W01_Boss_BloomEngine.Definition, W01_Boss_BloomEngine.Safe, W01_Boss_BloomEngine.Skilled),
        };

        public static IReadOnlyList<W01StageRouteContract> All => Contracts;

        public static W01StageRouteContract Get(string stageId)
        {
            if (string.IsNullOrWhiteSpace(stageId)) throw new ArgumentException("Stage id is required.", nameof(stageId));

            for (int i = 0; i < Contracts.Length; i++)
            {
                if (string.Equals(Contracts[i].Stage.Id, stageId, StringComparison.Ordinal))
                    return Contracts[i];
            }

            throw new KeyNotFoundException($"Unknown World 1 stage id: {stageId}");
        }
    }
}
