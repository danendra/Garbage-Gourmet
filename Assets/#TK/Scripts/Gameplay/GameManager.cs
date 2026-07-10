using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace TK.Gameplay
{
    using Data;

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Data")]
        [SerializeField] private RecipeData[] _arrRecipes;
        [SerializeField] private UpgradeData _upgradeData;

        [Header("Raccoon Stage Thresholds")]
        [SerializeField] private int[] _arrRaccoonStageThresholds = { 400000, 800000, 1500000, 4000000, 8000000 };

        private int _armLevel;
        private int _pickUpLevel;
        private int _releaseLevel;

        public int[] RaccoonStageThresholds => _arrRaccoonStageThresholds;

        public int intCurrentPoint { get; private set; }
        public int intCummulativePoint { get; private set; }
        public int intCurrentMoney => UpgradeSaveSystem.LoadCoins();
        public int ArmLevel => _armLevel;
        public int PickUpLevel => _pickUpLevel;
        public int ReleaseLevel => _releaseLevel;
        public int PlayerCoins => UpgradeSaveSystem.LoadCoins();
        public RecipeData[] GetAllRecipes => _arrRecipes;

        public bool SplashCompleted { get; private set; } = false;

        public event System.Action<int> OnArmUpgraded;
        public event System.Action<int> OnPickUpUpgraded;
        public event System.Action<int> OnReleaseUpgraded;

        public event System.Action OnPointUpdate;        

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            ResetGameData();

            Initialize();

            Application.targetFrameRate = 100;
        }

        public void Start()
        {
            if(!FTUESaveSystem.LoadFTUEFirstLaunchCompleted())
            {
                SceneManager.LoadScene("GameScene");
            }
        }

        public void Initialize()
        {
            intCurrentPoint = PlayerPrefs.GetInt("CURRENT_POINT");
            intCummulativePoint = PlayerPrefs.GetInt("CUMMULATIVE_POINT");

            if (!PlayerPrefs.HasKey("player_coins"))
                UpgradeSaveSystem.SaveCoins(0);

            LoadUpgrades();
        }

        public void ResetGameData()
        {
            UpgradeSaveSystem.ResetAll();
            FTUESaveSystem.ResetAll();

            foreach (var recipe in GetAllRecipes)
            {
                if (recipe != null)
                {
                    PlayerPrefs.DeleteKey("BURGER_" + recipe.name);
                }
            }

            intCurrentPoint = 0;
            intCummulativePoint = 0;
            UpgradeSaveSystem.SaveCoins(0);
            PlayerPrefs.DeleteKey("CURRENT_POINT");
            PlayerPrefs.DeleteKey("CUMMULATIVE_POINT");
            PlayerPrefs.DeleteKey("NEW_COLLECTION_UNLOCKED_FLAG");
            PlayerPrefs.Save();

            Debug.Log("Game data has been reset.");
        }

        #region Upgrade
        //Panggil waktu start game scene
        public void ApplyUpgradesToPlayer(HandMovement hand, PlayerInventory inventory)
        {
            if (hand != null)
                hand.SetArmLength(_upgradeData.GetArmLength(_armLevel));

            if (inventory != null)
            {
                inventory.SetMaxPickUpItems(_upgradeData.GetMaxPickUp(_pickUpLevel));
                inventory.SetReleaseChance(_upgradeData.GetMaxRelease(_releaseLevel));
            }
        }

        public void ResetUpgrades()
        {
            _armLevel = 0;
            _pickUpLevel = 0;
            _releaseLevel = 0;
            UpgradeSaveSystem.ResetAll();

            LoadUpgrades();
        }

        private void LoadUpgrades()
        {
            _armLevel = UpgradeSaveSystem.LoadArmLevel();
            _pickUpLevel = UpgradeSaveSystem.LoadPickUpLevel();
            _releaseLevel = UpgradeSaveSystem.LoadReleaseLevel();

            // Arm level start di level 1 defaultnya
            _armLevel = Mathf.Clamp(_armLevel, 0, _upgradeData.MaxArmLevel);
            _pickUpLevel = Mathf.Clamp(_pickUpLevel, 0, _upgradeData.MaxPickUpLevel);
            _releaseLevel = Mathf.Clamp(_releaseLevel, 0, _upgradeData.MaxReleaseLevel);
        }

        public void ReloadUpgradeLevels()
        {
            LoadUpgrades();
        }
        #endregion

        #region  Scoring

        public void AddPoint(int _intPoint)
        {
            intCurrentPoint += _intPoint;

            if (_intPoint > 0)
                intCummulativePoint += _intPoint;

            PlayerPrefs.SetInt("CURRENT_POINT", intCurrentPoint);
            PlayerPrefs.SetInt("CUMMULATIVE_POINT", intCummulativePoint);

            if (_intPoint > 0)
            {
                int currentCoins = UpgradeSaveSystem.LoadCoins();
                UpgradeSaveSystem.SaveCoins(currentCoins + _intPoint);
            }

            OnPointUpdate?.Invoke();
        }

        #endregion

        #region Change Scene

        public void LoadScene(int _intIndex)
        {
            SceneManager.LoadScene(_intIndex);
        }

        public void LoadScene(string _strScene)
        {
            SceneManager.LoadScene(_strScene);
        }

        public void Restart()
        {
            LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        #endregion

        public void SetSplashCompleted(bool _bCompleted)
        {
            SplashCompleted = _bCompleted;
        }
    }
}
