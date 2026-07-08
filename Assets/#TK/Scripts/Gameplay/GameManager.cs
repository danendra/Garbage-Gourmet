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

        private int _playerMoney;

        private int _armLevel;
        private int _pickUpLevel;
        private int _releaseLevel;

        public int intCurrentPoint { get; private set; }
        public int intCummulativePoint { get; private set; }
        public int ArmLevel => _armLevel;
        public int PickUpLevel => _pickUpLevel;
        public int ReleaseLevel => _releaseLevel;

        public bool CanUpgradeArm => _armLevel < _upgradeData.MaxArmLevel && _playerMoney >= _upgradeData.ArmLengthLevels[_armLevel + 1].Cost;
        public bool CanUpgradePickUp => _pickUpLevel < _upgradeData.MaxPickUpLevel && _playerMoney >= _upgradeData.MaxPickUpLevels[_pickUpLevel + 1].Cost;
        public bool CanUpgradeRelease => _releaseLevel < _upgradeData.MaxReleaseLevel && _playerMoney >= _upgradeData.MaxReleaseLevels[_releaseLevel + 1].Cost;
        public RecipeData[] GetAllRecipes => _arrRecipes;

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

            Initialize();
        }

        public void Initialize()
        {
            intCurrentPoint = PlayerPrefs.GetInt("CURRENT_POINT");
            intCummulativePoint = PlayerPrefs.GetInt("CUMMULATIVE_POINT");

            LoadUpgrades();
        }

        #region Upgrade
        //Panggil waktu start game scene
        public void ApplyUpgradesToPlayer(HandMovement hand, PlayerInventory inventory)
        {
            if (hand != null)
                hand.SetArmLength(_upgradeData.GetArmLength(_armLevel));

            if (inventory != null)
                inventory.SetMaxPickUpItems(_upgradeData.GetMaxPickUp(_pickUpLevel));
        }

        public bool UpgradeArm()
        {
            if (!CanUpgradeArm) return false;

            _armLevel++;
            UpgradeSaveSystem.SaveArmLevel(_armLevel);
            OnArmUpgraded?.Invoke(_armLevel);
            return true;
        }

        public bool UpgradePickUp()
        {
            if (!CanUpgradePickUp) return false;

            _pickUpLevel++;
            UpgradeSaveSystem.SavePickUpLevel(_pickUpLevel);
            OnPickUpUpgraded?.Invoke(_pickUpLevel);
            return true;
        }

        public bool UpgradeRelease()
        {
            if (!CanUpgradeRelease) return false;

            _releaseLevel++;
            UpgradeSaveSystem.SaveReleaseLevel(_releaseLevel);
            OnReleaseUpgraded?.Invoke(_releaseLevel);
            return true;
        }

        public int PeekNextArmLength()
        {
            return _upgradeData.GetArmLength(_armLevel + 1);
        }

        public int PeekNextMaxPickUp()
        {
            return _upgradeData.GetMaxPickUp(_pickUpLevel + 1);
        }

        public int PeekNextMaxRelease()
        {
            return _upgradeData.GetMaxRelease(_releaseLevel + 1);
        }

        public void ResetUpgrades()
        {
            _armLevel = 0;
            _pickUpLevel = 0;
            _releaseLevel = 0;
            UpgradeSaveSystem.ResetAll();
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
        #endregion

        #region  Scoring

        public void AddPoint(int _intPoint)
        {   
            intCurrentPoint += _intPoint;

            if (_intPoint > 0)
                intCummulativePoint += _intPoint;

            PlayerPrefs.SetInt("CURRENT_POINT", intCurrentPoint);
            PlayerPrefs.SetInt("CUMMULATIVE_POINT", intCummulativePoint);

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
    }
}
