using NUnit.Framework;
using UnityEngine;
using Veil.Movement;
using Veil.Movement.Actions;
using Veil.Movement.States;

namespace Veil.Tests.EditMode
{
    public class ActionControllerTests
    {
        private class FakeAction : IMovementAction
        {
            public int Priority { get; set; }
            public bool IsActive { get; private set; }
            public bool AllowExecute { get; set; } = true;
            public int ExecuteCount { get; private set; }
            public int CancelCount { get; private set; }

            public bool CanExecute(MovementContext ctx, MovementStateId currentState) => AllowExecute && !IsActive;
            public void Execute(MovementContext ctx) { IsActive = true; ExecuteCount++; }
            public void Tick(MovementContext ctx) { }
            public void Cancel(MovementContext ctx) { IsActive = false; CancelCount++; }
        }

        [Test]
        public void ExecutesHighestPriorityValidAction()
        {
            var low = new FakeAction { Priority = 1 };
            var high = new FakeAction { Priority = 10 };
            var controller = new ActionController();
            controller.RegisterAction(low);
            controller.RegisterAction(high);

            controller.TryTriggerBestAction(null, MovementStateId.Grounded);

            Assert.AreEqual(1, high.ExecuteCount);
            Assert.AreEqual(0, low.ExecuteCount);
        }

        [Test]
        public void ActiveAction_BlocksLowerPriorityFromInterrupting()
        {
            var running = new FakeAction { Priority = 10 };
            var interrupter = new FakeAction { Priority = 5 };
            var controller = new ActionController();
            controller.RegisterAction(running);
            controller.RegisterAction(interrupter);

            controller.TryTriggerBestAction(null, MovementStateId.Grounded);
            controller.TryTriggerBestAction(null, MovementStateId.Grounded);

            Assert.AreEqual(1, running.ExecuteCount);
            Assert.AreEqual(0, interrupter.ExecuteCount);
        }
    }
}
