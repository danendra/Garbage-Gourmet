#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TK.Managers;
using TK.UI;
using TK.Data;

namespace TK.UI
{
    public static class MainMenuSetup
    {
        [MenuItem("Tools/TK/Link Existing Main Menu UI (Preserves Hierarchy)")]
        public static void LinkExistingUI()
        {
            // Find PageManager in the active scene
            PageManager pageManager = Object.FindFirstObjectByType<PageManager>();
            if (pageManager == null)
            {
                // Try to find Canvas to attach PageManager if not found
                Canvas canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject pmGo = new GameObject("PageManager", typeof(PageManager));
                    Undo.RegisterCreatedObjectUndo(pmGo, "Create PageManager");
                    pmGo.transform.SetParent(canvas.transform, false);
                    pageManager = pmGo.GetComponent<PageManager>();
                    Debug.Log("Created PageManager GameObject since it was missing.");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "No Canvas or PageManager found in the scene! Please open the MainMenu scene.", "OK");
                    return;
                }
            }

            Undo.RegisterFullObjectHierarchyUndo(pageManager.gameObject, "Link Main Menu UI References");

            // Find all MainMenuNavigationButton components in the active scene
            MainMenuNavigationButton[] navButtons = Object.FindObjectsByType<MainMenuNavigationButton>(FindObjectsSortMode.None);
            if (navButtons.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "No MainMenuNavigationButton components found in the scene. Please make sure the buttons have the MainMenuNavigationButton script attached.", "OK");
                return;
            }

            MainMenuNavigationButton stallBtn = null;
            MainMenuNavigationButton upgradeBtn = null;
            MainMenuNavigationButton settingsBtn = null;

            foreach (var button in navButtons)
            {
                Undo.RegisterFullObjectHierarchyUndo(button.gameObject, "Link Button References");

                Transform overlay = null;
                Transform icon = null;
                Transform text = null;

                // Search children for SelectedOverlay, Icon, and Text
                foreach (Transform child in button.transform)
                {
                    string nameLower = child.name.ToLower();
                    
                    // Check if it's text (TextMeshPro or Standard Text)
                    if (child.GetComponent<TextMeshProUGUI>() != null || child.GetComponent<Text>() != null)
                    {
                        text = child;
                    }
                    // Check if it's the SelectedOverlay layer (SelectedPage)
                    else if (nameLower.Contains("overlay") || nameLower.Contains("selected") || nameLower.Contains("active"))
                    {
                        overlay = child;
                    }
                    // Check if it's the Icon (usually has "icon" or matches features)
                    else if (nameLower.Contains("icon") || nameLower.Contains("stall") || nameLower.Contains("powerup") || nameLower.Contains("settings") || nameLower.Contains("upgrade"))
                    {
                        icon = child;
                    }
                }

                // If not found by name, fallback to children with image components (non-buttons, non-text)
                if (icon == null)
                {
                    foreach (Transform child in button.transform)
                    {
                        if (child != overlay && child != text && child.GetComponent<Image>() != null)
                        {
                            icon = child;
                            break;
                        }
                    }
                }

                // Apply references to the MainMenuNavigationButton component
                SerializedObject serializedBtn = new SerializedObject(button);
                if (overlay != null)
                {
                    serializedBtn.FindProperty("selectedOverlay").objectReferenceValue = overlay.gameObject;
                    Debug.Log($"Linked '{overlay.name}' as SelectedOverlay on button '{button.name}'", button);
                }
                if (icon != null)
                {
                    serializedBtn.FindProperty("iconRect").objectReferenceValue = icon.GetComponent<RectTransform>();
                    Debug.Log($"Linked '{icon.name}' as Icon on button '{button.name}'", button);
                }
                if (text != null)
                {
                    serializedBtn.FindProperty("textRect").objectReferenceValue = text.GetComponent<RectTransform>();
                    Debug.Log($"Linked '{text.name}' as Text on button '{button.name}'", button);
                }

                // Disable size overwriting to fully preserve the user's manual layout sizes!
                serializedBtn.FindProperty("adjustSizeAtRuntime").boolValue = false;
                serializedBtn.ApplyModifiedProperties();

                // Map button type to assign to PageManager
                int pageTypeVal = serializedBtn.FindProperty("targetPage").enumValueIndex;
                if (pageTypeVal == (int)MainMenuNavigationButton.PageType.Stall) stallBtn = button;
                else if (pageTypeVal == (int)MainMenuNavigationButton.PageType.Upgrade) upgradeBtn = button;
                else if (pageTypeVal == (int)MainMenuNavigationButton.PageType.Settings) settingsBtn = button;
            }

            // Hook references inside PageManager
            SerializedObject serManager = new SerializedObject(pageManager);
            if (stallBtn != null) serManager.FindProperty("stallButton").objectReferenceValue = stallBtn;
            if (upgradeBtn != null) serManager.FindProperty("upgradeButton").objectReferenceValue = upgradeBtn;
            if (settingsBtn != null) serManager.FindProperty("settingsButton").objectReferenceValue = settingsBtn;

            // Find config and link it
            MainMenuUIConfig config = AssetDatabase.LoadAssetAtPath<MainMenuUIConfig>("Assets/#TK/Settings/MainMenuUIConfig.asset");
            if (config != null)
            {
                serManager.FindProperty("uiConfig").objectReferenceValue = config;
            }
            serManager.ApplyModifiedProperties();

            EditorUtility.SetDirty(pageManager.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(pageManager.gameObject.scene);

            EditorUtility.DisplayDialog("Success", "All manual UI layout references linked successfully! No hierarchy, sizes, scales, or fonts were altered.", "Awesome");
        }

        [MenuItem("Tools/TK/Auto-Setup Main Menu UI (Creates New)")]
        public static void SetupMainMenuUI()
        {
            // Find the Canvas in the active scene
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Setup Error", "No Canvas found in the active scene! Please open the MainMenu scene first.", "OK");
                return;
            }

            // Load or create MainMenuUIConfig ScriptableObject
            string configPath = "Assets/#TK/Settings/MainMenuUIConfig.asset";
            MainMenuUIConfig config = AssetDatabase.LoadAssetAtPath<MainMenuUIConfig>(configPath);
            if (config == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/#TK/Settings"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/#TK"))
                    {
                        AssetDatabase.CreateFolder("Assets", "#TK");
                    }
                    AssetDatabase.CreateFolder("Assets/#TK", "Settings");
                }
                config = ScriptableObject.CreateInstance<MainMenuUIConfig>();
                AssetDatabase.CreateAsset(config, configPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Created default MainMenuUIConfig asset at {configPath}");
            }

            // Automatically resolve and assign custom sprites if they aren't configured yet
            Sprite unselectedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/#TK/Sprite/UI/ButtonPlaceholder.png");
            Sprite selectedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/#TK/Sprite/UI/SelectedPage.png");
            if (config != null)
            {
                config.SetDefaultSprites(unselectedSprite, selectedSprite);
                EditorUtility.SetDirty(config);
            }

            // Automatically resolve and assign icon sprites if they aren't configured yet
            Sprite stallIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/#TK/Sprite/UI/IconStall.png");
            Sprite upgradeIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/#TK/Sprite/UI/IconPowerUpIcon.png");
            Sprite settingsIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/#TK/Sprite/UI/IconSettings.png");
            if (config != null)
            {
                config.SetDefaultIcons(stallIcon, upgradeIcon, settingsIcon);
                EditorUtility.SetDirty(config);
            }

            // Register undo point for the canvas and its children
            Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "Setup Main Menu UI");

            // Configure CanvasScaler for dynamic screen sizes (Scale With Screen Size, 1920x1080 landscape)
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                Undo.RecordObject(scaler, "Configure CanvasScaler");
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            // Locate existing TitleGroup to preserve it
            Transform titleGroup = canvas.transform.Find("TitleGroup");

            // Clean up existing generated components (idempotency check)
            Transform existingContentArea = canvas.transform.Find("ContentArea");
            if (existingContentArea != null)
            {
                // If TitleGroup was nested inside StallPage, extract it back to Canvas root to preserve it
                Transform nestedTitleGroup = existingContentArea.Find("StallPage/TitleGroup");
                if (nestedTitleGroup != null)
                {
                    titleGroup = nestedTitleGroup;
                    titleGroup.SetParent(canvas.transform);
                }
                Undo.DestroyObjectImmediate(existingContentArea.gameObject);
            }

            Transform existingBottomNav = canvas.transform.Find("BottomNavigation");
            if (existingBottomNav != null)
            {
                Undo.DestroyObjectImmediate(existingBottomNav.gameObject);
            }

            Transform existingPageManager = canvas.transform.Find("PageManager");
            if (existingPageManager != null)
            {
                Undo.DestroyObjectImmediate(existingPageManager.gameObject);
            }

            if (titleGroup == null)
            {
                EditorUtility.DisplayDialog("Setup Warning", "TitleGroup was not found on the root of the Canvas. If it was renamed or deleted, please assign it manually to StallPage.", "OK");
            }

            // 1. Create ContentArea (stretched)
            GameObject contentAreaGo = new GameObject("ContentArea", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(contentAreaGo, "Create ContentArea");
            contentAreaGo.transform.SetParent(canvas.transform, false);
            RectTransform contentAreaRect = contentAreaGo.GetComponent<RectTransform>();
            StretchRectTransform(contentAreaRect);

            // 2. Create StallPage
            GameObject stallPageGo = new GameObject("StallPage", typeof(RectTransform));
            stallPageGo.transform.SetParent(contentAreaRect, false);
            StretchRectTransform(stallPageGo.GetComponent<RectTransform>());

            // Move TitleGroup under StallPage if it exists
            if (titleGroup != null)
            {
                titleGroup.SetParent(stallPageGo.transform, false);
            }

            // 3. Create UpgradePage (disabled by default)
            GameObject upgradePageGo = new GameObject("UpgradePage", typeof(RectTransform));
            upgradePageGo.transform.SetParent(contentAreaRect, false);
            StretchRectTransform(upgradePageGo.GetComponent<RectTransform>());
            CreatePlaceholderText(upgradePageGo.transform, "UPGRADE PAGE\n\n<Placeholder UI - Upgrades Coming Soon!>");
            upgradePageGo.SetActive(false);

            // 4. Create SettingsPage (disabled by default)
            GameObject settingsPageGo = new GameObject("SettingsPage", typeof(RectTransform));
            settingsPageGo.transform.SetParent(contentAreaRect, false);
            StretchRectTransform(settingsPageGo.GetComponent<RectTransform>());
            CreatePlaceholderText(settingsPageGo.transform, "SETTINGS PAGE\n\n<Placeholder UI - Settings Coming Soon!>");
            settingsPageGo.SetActive(false);

            // 5. Create BottomNavigation (using unselected sprite as background element base)
            GameObject bottomNavGo = new GameObject("BottomNavigation", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            Undo.RegisterCreatedObjectUndo(bottomNavGo, "Create BottomNavigation");
            bottomNavGo.transform.SetParent(canvas.transform, false);
            RectTransform bottomNavRect = bottomNavGo.GetComponent<RectTransform>();
            
            // Layout styling using config ScriptableObject
            bottomNavRect.anchorMin = new Vector2(0.5f, 0f);
            bottomNavRect.anchorMax = new Vector2(0.5f, 0f);
            bottomNavRect.pivot = new Vector2(0.5f, 0f);
            bottomNavRect.anchoredPosition = config.BottomNavOffset;
            bottomNavRect.sizeDelta = config.BottomNavSize;

            Image navBgImage = bottomNavGo.GetComponent<Image>();
            navBgImage.sprite = config.UnselectedSprite;
            navBgImage.type = Image.Type.Sliced;
            navBgImage.color = config.BackgroundColor;

            HorizontalLayoutGroup layout = bottomNavGo.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = config.ButtonSpacing;
            // Set alignment to LowerCenter so tabs line up flat at the bottom and pop up taller when selected
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(20, 20, 10, 10);

            // 6. Create Navigation Buttons: LEFT = Upgrade, MIDDLE = Stall, RIGHT = Settings
            GameObject upgradeBtn = CreateNavigationButton("UpgradeButton", bottomNavRect.transform, config.UpgradeLabel, MainMenuNavigationButton.PageType.Upgrade, config.UnselectedSprite, config.UpgradeIconSprite, config);
            GameObject stallBtn = CreateNavigationButton("StallButton", bottomNavRect.transform, config.StallLabel, MainMenuNavigationButton.PageType.Stall, config.UnselectedSprite, config.StallIconSprite, config);
            GameObject settingsBtn = CreateNavigationButton("SettingsButton", bottomNavRect.transform, config.SettingsLabel, MainMenuNavigationButton.PageType.Settings, config.UnselectedSprite, config.SettingsIconSprite, config);

            // 7. Create PageManager GameObject and component
            GameObject pageManagerGo = new GameObject("PageManager", typeof(PageManager));
            Undo.RegisterCreatedObjectUndo(pageManagerGo, "Create PageManager");
            pageManagerGo.transform.SetParent(canvas.transform, false);

            PageManager pageManager = pageManagerGo.GetComponent<PageManager>();
            SerializedObject serializedManager = new SerializedObject(pageManager);
            serializedManager.FindProperty("stallPage").objectReferenceValue = stallPageGo;
            serializedManager.FindProperty("upgradePage").objectReferenceValue = upgradePageGo;
            serializedManager.FindProperty("settingsPage").objectReferenceValue = settingsPageGo;
            serializedManager.FindProperty("uiConfig").objectReferenceValue = config;
            serializedManager.FindProperty("stallButton").objectReferenceValue = stallBtn.GetComponent<MainMenuNavigationButton>();
            serializedManager.FindProperty("upgradeButton").objectReferenceValue = upgradeBtn.GetComponent<MainMenuNavigationButton>();
            serializedManager.FindProperty("settingsButton").objectReferenceValue = settingsBtn.GetComponent<MainMenuNavigationButton>();
            serializedManager.ApplyModifiedProperties();

            // Re-order components hierarchy for neatness
            bottomNavGo.transform.SetSiblingIndex(titleGroup != null ? titleGroup.GetSiblingIndex() + 1 : 3);
            pageManagerGo.transform.SetAsLastSibling();

            EditorUtility.SetDirty(canvas.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            EditorUtility.DisplayDialog("Success", "Main Menu Page-Based Navigation UI Setup completed successfully!", "Awesome");
        }

        private static void StretchRectTransform(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void CreatePlaceholderText(Transform parent, string text)
        {
            GameObject textGo = new GameObject("PlaceholderText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(parent, false);
            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(800f, 300f);
            textRect.anchoredPosition = new Vector2(0f, 50f);

            TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 28f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.9f, 0.9f, 0.9f, 0.85f);
        }

        private static GameObject CreateNavigationButton(string name, Transform parent, string label, MainMenuNavigationButton.PageType pageType, Sprite btnSprite, Sprite iconSprite, MainMenuUIConfig config)
        {
            GameObject btnGo = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(MainMenuNavigationButton));
            btnGo.transform.SetParent(parent, false);
            RectTransform rect = btnGo.GetComponent<RectTransform>();
            
            // Set pivot to bottom-center (0.5, 0) so that changes in height cause it to grow upwards
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = config.UnselectedSize;

            Image img = btnGo.GetComponent<Image>();
            img.sprite = btnSprite;
            img.type = Image.Type.Sliced;
            img.color = config.ButtonNormalColor;

            Button button = btnGo.GetComponent<Button>();
            ColorBlock cb = button.colors;
            cb.normalColor = config.ButtonNormalColor;
            cb.highlightedColor = config.ButtonHighlightedColor;
            cb.pressedColor = config.ButtonPressedColor;
            cb.selectedColor = config.ButtonSelectedColor;
            button.colors = cb;

            // 1. Create SelectedOverlay child GameObject (using SelectedPage sprite)
            GameObject overlayGo = new GameObject("SelectedOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlayGo.transform.SetParent(btnGo.transform, false);
            RectTransform overlayRect = overlayGo.GetComponent<RectTransform>();
            StretchRectTransform(overlayRect);

            Image overlayImg = overlayGo.GetComponent<Image>();
            overlayImg.sprite = config.SelectedSprite;
            overlayImg.type = Image.Type.Sliced;
            overlayImg.color = config.ButtonSelectedColor;
            overlayGo.SetActive(false);

            // 2. Create Icon child GameObject
            GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(btnGo.transform, false);
            RectTransform iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0f);
            iconRect.anchorMax = new Vector2(0.5f, 0f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(60f, 60f);
            // Default position inside unselected button
            iconRect.anchoredPosition = new Vector2(0f, config.UnselectedSize.y / 2f);
            iconRect.localScale = new Vector3(config.UnselectedIconScale, config.UnselectedIconScale, 1f);

            Image iconImg = iconGo.GetComponent<Image>();
            iconImg.sprite = iconSprite;
            iconImg.preserveAspect = true;

            // 3. Add text child (rendered on top of overlay & icon)
            GameObject txtGo = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(btnGo.transform, false);
            RectTransform txtRect = txtGo.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = txtGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 18f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            txtGo.SetActive(false);

            // Configure navigation script
            MainMenuNavigationButton navBtn = btnGo.GetComponent<MainMenuNavigationButton>();
            SerializedObject serializedBtn = new SerializedObject(navBtn);
            serializedBtn.FindProperty("targetPage").enumValueIndex = (int)pageType;
            serializedBtn.FindProperty("selectedOverlay").objectReferenceValue = overlayGo;
            serializedBtn.FindProperty("iconRect").objectReferenceValue = iconRect;
            serializedBtn.FindProperty("textRect").objectReferenceValue = txtRect;
            serializedBtn.FindProperty("adjustSizeAtRuntime").boolValue = true; // Enabled for new setups
            serializedBtn.ApplyModifiedProperties();

            return btnGo;
        }
    }
}
#endif
