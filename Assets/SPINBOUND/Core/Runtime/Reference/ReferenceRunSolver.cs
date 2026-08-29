using System;
using System.Collections.Generic;
using System.Numerics;
using Spinbound.Core.Collision;
using Spinbound.Core.Simulation;

namespace Spinbound.Core.Reference
{
    public static class ReferenceRunSolver
    {
        public static ReferenceRunResult Solve(
            RotorState initialState,
            IReadOnlyList<CourseCollider> colliders,
            IReadOnlyList<ReferenceAction> actions,
            Vector2 finishCenter,
            float finishRadius)
        {
            var world = new CollisionWorld(colliders);
            var state = initialState;
            var elapsed = 0f;
            var minClearance = float.PositiveInfinity;
            var initialContact = world.TestCapsule(ToCapsule(state));
            if (initialContact.Hit) return new ReferenceRunResult(false, 1, 0f, initialContact.ClearanceMeters, "spawn-collision");

            for (var actionIndex = 0; actionIndex < actions.Count; actionIndex++)
            {
                var action = actions[actionIndex];
                if (action.Kind == ReferenceActionKind.Wait)
                {
                    var waitSteps = (int)MathF.Ceiling(action.Seconds * RotorTuning.FixedHz);
                    for (var i = 0; i < waitSteps; i++)
                    {
                        if (!Step(ref state, RotorIntent.Idle, world, ref elapsed, ref minClearance, out var failure))
                            return new ReferenceRunResult(false, 1, elapsed, minClearance, failure);
                    }
                    continue;
                }

                var budget = (int)MathF.Ceiling(action.Seconds * RotorTuning.FixedHz);
                var reached = false;
                for (var i = 0; i < budget; i++)
                {
                    var delta = action.Target - state.Position;
                    if (delta.LengthSquared() <= 0.0009f)
                    {
                        reached = true;
                        break;
                    }

                    var direction = Vector2.Normalize(delta);
                    var intent = new RotorIntent(direction, action.Tier);
                    if (!Step(ref state, intent, world, ref elapsed, ref minClearance, out var failure))
                        return new ReferenceRunResult(false, 1, elapsed, minClearance, failure);
                }

                if (!reached && Vector2.DistanceSquared(state.Position, action.Target) > 0.0036f)
                    return new ReferenceRunResult(false, 0, elapsed, minClearance, $"action-timeout-{actionIndex}");
            }

            var cleared = Vector2.DistanceSquared(state.Position, finishCenter) <= finishRadius * finishRadius;
            return new ReferenceRunResult(cleared, 0, elapsed, minClearance, cleared ? string.Empty : "finish-not-reached");
        }

        private static bool Step(ref RotorState state, in RotorIntent intent, CollisionWorld world, ref float elapsed, ref float minClearance, out string failure)
        {
            var candidate = RotorSimulation.Step(state, intent, RotorTuning.FixedDeltaSeconds);
            var collision = world.SweepCapsule(ToCapsule(state), ToCapsule(candidate));
            minClearance = MathF.Min(minClearance, collision.ClearanceMeters);
            elapsed += RotorTuning.FixedDeltaSeconds;
            if (collision.Hit)
            {
                failure = $"collision:{collision.ColliderId}";
                return false;
            }
            state = candidate;
            failure = string.Empty;
            return true;
        }

        private static Capsule2D ToCapsule(in RotorState state) =>
            new(state.Position, state.AngleDeg, state.HalfLengthMeters, state.RadiusMeters);
    }
}
