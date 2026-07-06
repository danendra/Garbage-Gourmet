using UnityEngine;
using TK.Data;
using TK.Gameplay;

namespace TK.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Upgrade Data")]
        [SerializeField] private UpgradeData _upgradeData;

        private int _armLevel;
        private int _pickUpLevel;

        public int ArmLevel    => _armLevel;
        public int PickUpLevel => _pickUpLevel;

        public bool CanUpgradeArm    => _armLevel    < _upgradeData.MaxArmLevel;
        public bool CanUpgradePickUp => _pickUpLevel < _upgradeData.MaxPickUpLevel;

        public event System.Action<int> OnArmUpgraded;
        public event System.Action<int> OnPickUpUpgraded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadUpgrades();
        }

        //Panggil waktu start game scene
        public void ApplyUpgradesToPlayer(PlayerMovement arm, PlayerInventory inventory)
        {
            if (arm != null)
                arm.SetArmLength(_upgradeData.GetArmLength(_armLevel));

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

        public int PeekNextArmLength()
        {
            return _upgradeData.GetArmLength(_armLevel + 1);
        }

        public int PeekNextMaxPickUp()
        {
            return _upgradeData.GetMaxPickUp(_pickUpLevel + 1);
        }

        public void ResetUpgrades()
        {
            _armLevel    = 0;
            _pickUpLevel = 0;
            UpgradeSaveSystem.ResetAll();

            Debug.Log($"Upgrades reset. Arm level: {PlayerPrefs.GetInt("upgrade_arm_level", 0)}, Pick up level: {PlayerPrefs.GetInt("upgrade_pickup_level", 0)}");
        }

        private void LoadUpgrades()
        {
            _armLevel    = UpgradeSaveSystem.LoadArmLevel();
            _pickUpLevel = UpgradeSaveSystem.LoadPickUpLevel();

            // Clamp in case the SO tier count was reduced after saving
            _armLevel    = Mathf.Clamp(_armLevel,    0, _upgradeData.MaxArmLevel);
            _pickUpLevel = Mathf.Clamp(_pickUpLevel, 0, _upgradeData.MaxPickUpLevel);
        }

        // INI BUAT TESTING
        public void UpgradeArmLevel()
        {
            if (!CanUpgradeArm) return;

            _armLevel++;
            UpgradeSaveSystem.SaveArmLevel(_armLevel);
            OnArmUpgraded?.Invoke(_armLevel);

            Debug.Log($"Arm level upgraded to {PlayerPrefs.GetInt("upgrade_arm_level", 0)}");
        }

        public void UpgradePickUpLevel()
        {
            if (!CanUpgradePickUp) return;

            _pickUpLevel++;
            UpgradeSaveSystem.SavePickUpLevel(_pickUpLevel);
            OnPickUpUpgraded?.Invoke(_pickUpLevel);

            Debug.Log($"Pick up level upgraded to {PlayerPrefs.GetInt("upgrade_pickup_level", 0)}");
        }

        public void DegradeArmLevel()
        {
            if (_armLevel <= 0) return;

            _armLevel--;
            UpgradeSaveSystem.SaveArmLevel(_armLevel);
            OnArmUpgraded?.Invoke(_armLevel);

            Debug.Log($"Arm level degraded to {PlayerPrefs.GetInt("upgrade_arm_level", 0)}");
        }

        public void DegradePickUpLevel()
        {
            if (_pickUpLevel <= 0) return;

            _pickUpLevel--;
            UpgradeSaveSystem.SavePickUpLevel(_pickUpLevel);
            OnPickUpUpgraded?.Invoke(_pickUpLevel);

            Debug.Log($"Pick up level degraded to {PlayerPrefs.GetInt("upgrade_pickup_level", 0)}");
        }
    }
}
