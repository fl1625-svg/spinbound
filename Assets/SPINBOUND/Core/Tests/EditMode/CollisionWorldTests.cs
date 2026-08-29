using System.Numerics;
using NUnit.Framework;
using Spinbound.Core.Collision;

namespace Spinbound.Core.Tests.EditMode
{
    public sealed class CollisionWorldTests
    {
        [Test]
        public void EmptyWorld_IsClear()
        {
            var world = new CollisionWorld(System.Array.Empty<CourseCollider>());
            var result = world.TestCapsule(new Capsule2D(Vector2.Zero, 0f, 1.44f, 0.16f));
            Assert.That(result.Hit, Is.False);
        }

        [Test]
        public void NormalRotor_HitsObstacleThatAssistClears()
        {
            var world = new CollisionWorld(new[]
            {
                new CourseCollider("skill-edge", new Vector2(1.35f, -0.20f), new Vector2(1.80f, 0.20f))
            });

            var normal = world.TestCapsule(new Capsule2D(Vector2.Zero, 0f, 1.44f, 0.16f));
            var assist = world.TestCapsule(new Capsule2D(Vector2.Zero, 0f, 1.08f, 0.16f));

            Assert.That(normal.Hit, Is.True);
            Assert.That(assist.Hit, Is.False);
        }

        [Test]
        public void RoundedCapsuleCorner_GrazingUsesTrueDistanceNotExpandedBoxApproximation()
        {
            var hitWorld = new CollisionWorld(new[]
            {
                new CourseCollider("near-corner", new Vector2(1.55f, 0.10f), new Vector2(1.80f, 0.40f))
            });
            var clearWorld = new CollisionWorld(new[]
            {
                new CourseCollider("far-corner", new Vector2(1.58f, 0.10f), new Vector2(1.80f, 0.40f))
            });
            var capsule = new Capsule2D(Vector2.Zero, 0f, 1.44f, 0.16f);

            Assert.That(hitWorld.TestCapsule(capsule).Hit, Is.True);
            Assert.That(clearWorld.TestCapsule(capsule).Hit, Is.False);
        }

        [Test]
        public void SweepCapsule_CatchesThinObstacleBetweenStartAndEnd()
        {
            var world = new CollisionWorld(new[]
            {
                new CourseCollider("thin-wall", new Vector2(0.49f, -0.5f), new Vector2(0.51f, 0.5f))
            });
            var start = new Capsule2D(new Vector2(-1f, 0f), 90f, 0.2f, 0.08f);
            var end = new Capsule2D(new Vector2(1f, 0f), 90f, 0.2f, 0.08f);

            var result = world.SweepCapsule(start, end);
            Assert.That(result.Hit, Is.True);
            Assert.That(result.ColliderId, Is.EqualTo("thin-wall"));
            Assert.That(result.SweepT, Is.InRange(0f, 1f));
        }
    }
}
