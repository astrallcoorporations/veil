namespace Veil.Movement.States
{
    /// <summary>Coarse macro-locomotion state. Kept small deliberately — one-shot moves (vault/mantle/slide) live in the Actions layer, not here.</summary>
    public enum MovementStateId
    {
        Grounded,
        Air,
        Crouch
    }
}
