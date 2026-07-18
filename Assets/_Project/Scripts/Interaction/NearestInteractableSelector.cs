using System.Collections.Generic;

namespace Veil.Interaction
{
    /// <summary>Pure selection logic: picks the closest interactable from a candidate list.</summary>
    public static class NearestInteractableSelector
    {
        /// <summary>Returns the candidate with the smallest distance, or null if the list is empty.</summary>
        public static IInteractable SelectNearest(IReadOnlyList<(IInteractable interactable, float distance)> candidates)
        {
            if (candidates.Count == 0) return null;

            IInteractable best = candidates[0].interactable;
            float bestDistance = candidates[0].distance;
            for (int i = 1; i < candidates.Count; i++)
            {
                if (candidates[i].distance < bestDistance)
                {
                    bestDistance = candidates[i].distance;
                    best = candidates[i].interactable;
                }
            }
            return best;
        }
    }
}
