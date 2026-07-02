using UnityEngine;

namespace TK.Gameplay
{
    /// Implemented by any GameObject that can collect items from the world.
    public interface IItemCollector
    {
        bool CanCollect { get; }
        Transform HoldPoint { get; }
        void AddItem(CollectedItemData data);
    }
}
