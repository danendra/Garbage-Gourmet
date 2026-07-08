using UnityEngine;

namespace TK.Gameplay
{   
    // Ini buat load save upgrades tiap kali game scene mulai
    public class SessionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HandMovement _handMovement;
        [SerializeField] private PlayerInventory _inventoryManager;

        private void Start()
        {
            GameManager.Instance.ApplyUpgradesToPlayer(_handMovement, _inventoryManager);
            Debug.Log($"SessionManager: Applied upgrades to player. Arm Level: {GameManager.Instance.ArmLevel}, Pick Up Level: {GameManager.Instance.PickUpLevel}");
        }
    }
}

