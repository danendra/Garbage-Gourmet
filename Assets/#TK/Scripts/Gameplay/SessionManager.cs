using UnityEngine;

namespace TK.Gameplay
{   
    // Ini buat load save upgrades tiap kali game scene mulai
    public class SessionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerInventory _inventoryManager;


        private void Start()
        {
            GameManager.Instance.ApplyUpgradesToPlayer(_playerMovement, _inventoryManager);
        }
    }
}

