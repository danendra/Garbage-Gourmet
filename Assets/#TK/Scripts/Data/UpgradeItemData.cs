using UnityEngine;

namespace TK.Data
{
    [CreateAssetMenu(fileName = "NewUpgradeItem", menuName = "Garbage Gourmet/Upgrade Item")]
    public class UpgradeItemData : ScriptableObject
    {
        [Header("Upgrade Settings")]
        public string upgradeKey;
        public string title;
        [TextArea(3, 5)] public string description;
        public Sprite icon;
    }
}
