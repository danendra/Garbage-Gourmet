using UnityEngine;

namespace TK.Data
{
    /// <summary>
    /// Configurable UI settings for the Main Menu Page Navigation system.
    /// Allows designers to customize layout, colors, and text via ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "MainMenuUIConfig", menuName = "TK/Data/MainMenuUIConfig")]
    public class MainMenuUIConfig : ScriptableObject
    {
        [Header("Bottom Navigation Layout")]
        [SerializeField] private Vector2 bottomNavSize = new Vector2(800f, 100f);
        [SerializeField] private Vector2 bottomNavOffset = new Vector2(0f, 40f);
        [SerializeField] private float buttonSpacing = 30f;

        [Header("Tab Sprites & Responsive Sizes")]
        [SerializeField] private Sprite unselectedSprite;
        [SerializeField] private Sprite selectedSprite;
        [SerializeField] private Vector2 unselectedSize = new Vector2(200f, 80f);
        [SerializeField] private Vector2 selectedSize = new Vector2(200f, 100f);

        [Header("Tab Icons (Sprites)")]
        [SerializeField] private Sprite stallIconSprite;
        [SerializeField] private Sprite upgradeIconSprite;
        [SerializeField] private Sprite settingsIconSprite;

        [Header("Icon & Text Sizing & Offsets")]
        [Min(0f)] [SerializeField] private float selectedIconScale = 1.0f;
        [Min(0f)] [SerializeField] private float unselectedIconScale = 0.7f;
        [SerializeField] private float selectedTextTopOffset = 2f;
        [SerializeField] private float selectedTextBottomOffset = -102f;

        [Header("Tab Labels")]
        [SerializeField] private string stallLabel = "STALL";
        [SerializeField] private string upgradeLabel = "UPGRADE";
        [SerializeField] private string settingsLabel = "SETTINGS";

        [Header("Visual Theme")]
        [SerializeField] private Color backgroundColor = new Color(0.08f, 0.08f, 0.08f, 0.7f);
        [SerializeField] private Color buttonNormalColor = Color.white;
        [SerializeField] private Color buttonHighlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        [SerializeField] private Color buttonPressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        [SerializeField] private Color buttonSelectedColor = Color.white;

        // Public getters
        public Vector2 BottomNavSize => bottomNavSize;
        public Vector2 BottomNavOffset => bottomNavOffset;
        public float ButtonSpacing => buttonSpacing;

        public Sprite UnselectedSprite => unselectedSprite;
        public Sprite SelectedSprite => selectedSprite;
        public Vector2 UnselectedSize => unselectedSize;
        public Vector2 SelectedSize => selectedSize;

        public Sprite StallIconSprite => stallIconSprite;
        public Sprite UpgradeIconSprite => upgradeIconSprite;
        public Sprite SettingsIconSprite => settingsIconSprite;

        public float SelectedIconScale => selectedIconScale;
        public float UnselectedIconScale => unselectedIconScale;
        public float SelectedTextTopOffset => selectedTextTopOffset;
        public float SelectedTextBottomOffset => selectedTextBottomOffset;

        public string StallLabel => stallLabel;
        public string UpgradeLabel => upgradeLabel;
        public string SettingsLabel => settingsLabel;

        public Color BackgroundColor => backgroundColor;
        public Color ButtonNormalColor => buttonNormalColor;
        public Color ButtonHighlightedColor => buttonHighlightedColor;
        public Color ButtonPressedColor => buttonPressedColor;
        public Color ButtonSelectedColor => buttonSelectedColor;

        public void SetDefaultSprites(Sprite unselected, Sprite selected)
        {
            if (unselectedSprite == null) unselectedSprite = unselected;
            if (selectedSprite == null) selectedSprite = selected;
        }

        public void SetDefaultIcons(Sprite stallIcon, Sprite upgradeIcon, Sprite settingsIcon)
        {
            if (stallIconSprite == null) stallIconSprite = stallIcon;
            if (upgradeIconSprite == null) upgradeIconSprite = upgradeIcon;
            if (settingsIconSprite == null) settingsIconSprite = settingsIcon;
        }
    }
}
