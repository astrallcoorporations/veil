namespace Veil.Movement.Actions
{
    /// <summary>Which traversal move a detected ledge should trigger, if any.</summary>
    public enum LedgeActionType
    {
        /// <summary>No obstacle detected, or the obstacle is too tall to traverse.</summary>
        None,

        /// <summary>Obstacle is low enough to vault over.</summary>
        Vault,

        /// <summary>Obstacle is too tall to vault but low enough to mantle onto.</summary>
        Mantle
    }

    /// <summary>
    /// Pure decision logic for vault-vs-mantle-vs-none, given ledge geometry already
    /// resolved by a caller's capsule casts. Kept free of Physics calls so it is
    /// directly unit-testable with synthetic inputs.
    /// </summary>
    public static class LedgeDetector
    {
        /// <summary>Decides which traversal action a detected obstacle should trigger.</summary>
        /// <param name="forwardHit">Whether a forward capsule cast found an obstacle ahead.</param>
        /// <param name="ledgeHeight">Height of the detected obstacle's surface, in meters.</param>
        /// <param name="settings">Movement tuning data supplying the vault/mantle height thresholds.</param>
        public static LedgeActionType Decide(bool forwardHit, float ledgeHeight, MovementSettings settings)
        {
            if (!forwardHit) return LedgeActionType.None;
            if (ledgeHeight <= settings.VaultMaxHeight) return LedgeActionType.Vault;
            if (ledgeHeight <= settings.MantleMaxHeight) return LedgeActionType.Mantle;
            return LedgeActionType.None;
        }
    }
}
