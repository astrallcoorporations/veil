using System.Collections.Generic;
using NUnit.Framework;
using Veil.Interaction;

namespace Veil.Tests.EditMode
{
    public class NearestInteractableSelectorTests
    {
        private class FakeInteractable : IInteractable
        {
            public string GetPrompt() => "Fake";
            public void Interact(UnityEngine.GameObject interactor) { }
        }

        [Test]
        public void SelectNearest_ReturnsClosestByDistance()
        {
            var near = new FakeInteractable();
            var far = new FakeInteractable();
            var candidates = new List<(IInteractable, float)> { (far, 5f), (near, 1f) };

            var result = NearestInteractableSelector.SelectNearest(candidates);

            Assert.AreSame(near, result);
        }

        [Test]
        public void SelectNearest_EmptyList_ReturnsNull()
        {
            var candidates = new List<(IInteractable, float)>();
            Assert.IsNull(NearestInteractableSelector.SelectNearest(candidates));
        }
    }
}
