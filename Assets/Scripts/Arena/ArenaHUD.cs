using UnityEngine;
using UnityEngine.UI;

public class ArenaHUD : MonoBehaviour
{
    [Header("References - assign in Inspector")]
    public Text healthText;
    public Text armorText;
    public Text ammoText;

    private FPSController _player;

    void Start()
    {
        _player = FindObjectOfType<FPSController>();
        
        // Safety check if player isn't found immediately
        if (_player == null)
        {
            Debug.LogWarning("ArenaHUD: No FPSController found in scene!");
        }
    }

    void Update()
    {
        if (_player == null) return;

        // Update Health
        if (healthText != null) 
            healthText.text = _player.health.ToString();

        // Update Armor (with % symbol)
        if (armorText != null)  
            armorText.text = _player.armor.ToString() + "%";

        // Update Ammo (Fixed syntax: removed the duplicate .ammo)
        if (ammoText != null)   
            ammoText.text = _player.ammo.ToString();
    }
}