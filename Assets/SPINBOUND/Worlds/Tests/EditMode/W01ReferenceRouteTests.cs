using NUnit.Framework;
using Spinbound.Core.Reference;
using Spinbound.Core.Simulation;
using Spinbound.Worlds.W01.DaisyHighlands;

namespace Spinbound.Worlds.Tests.EditMode
{
    public sealed class W01ReferenceRouteTests
    {
        [TestCase(RotorMode.Standard)]
        [TestCase(RotorMode.Assist)]
        public void SafeRoute_ClearsWithoutContact(RotorMode mode)
        {
            var result = ReferenceRunSolver.Solve(
                W01_01CourseDefinition.StartFor(mode),
                W01_01CourseDefinition.Colliders,
                W01_01ReferenceRoute.Safe,
                W01_01CourseDefinition.FinishCenter,
                W01_01CourseDefinition.FinishRadius);

            Assert.That(result.Cleared, Is.True, result.Failure);
            Assert.That(result.Hits, Is.Zero);
            Assert.That(result.MinimumClearanceMeters, Is.GreaterThan(0f));
        }
    }
}
