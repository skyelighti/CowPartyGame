using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvasController : MainCanvasController
{
    [SerializeField] private Image greenFlash;
    [SerializeField] private Slider HealthSlider;
    [SerializeField] private Slider AbilityCDSlider;
    [SerializeField] private Slider InteractCDSlider;
    [SerializeField] private GameObject HurtIndicator;
    [SerializeField] private GameObject menu;

    // These are set by the PlayerController if it is the local player
    [HideInInspector] public PlayerController playerController;
    [HideInInspector] public GameObject playerObject;
    [HideInInspector] public LocalPlayerController localPlayerController;

    [SerializeField] private GameObject[] localActiveUi;

    private float lastHealth;

    [SerializeField] private float flashGreenMaxAlpha = 0.5f;
    private float flashGreenStartTime;
    private float flashGreenDuration;

    private bool menuIsShown;
    public bool MenuIsShown
    {
        get
        {
            Debug.Log($"Current value: {menuIsShown}");
            return menuIsShown;
        }
        private set
        {
            Debug.Log($"Setting to value: {value}");
            menuIsShown = value;
            menu.SetActive(value);
        }
    }

    public void UpdateHealthSlider()
    {
        HealthSlider.value = ((float)playerController.Health.Value) / playerController.maxHealth;
    }
    public void UpdateAbilitySlider()
    {
        PlayerAbilties[] abilityArray = playerObject.GetComponents<PlayerAbilties>();
        foreach (PlayerAbilties ability in abilityArray)
        {
            if (ability.enabled)
            {
                AbilityCDSlider.value = (float)ability.CDtime / ability.cooldown;
            }
        }
    }

    public void UpdateInteractSlider()
    {
        float val = localPlayerController.InteractCooldownNormalized;
        if (val == 1)
        {
            InteractCDSlider.gameObject.SetActive(false);
        }
        else
        {
            InteractCDSlider.gameObject.SetActive(true);
            InteractCDSlider.value = localPlayerController.InteractCooldownNormalized;
        }
    }

    private void UpdateGreenFlash()
    {
        greenFlash.color = new Color(
            greenFlash.color.r,
            greenFlash.color.g,
            greenFlash.color.b,
            Mathf.Lerp(flashGreenMaxAlpha, 0, (Time.time - flashGreenStartTime) / flashGreenDuration)
        );
    }

    private void HurtyIndicator()
    {
        if ((float)playerController.Health.Value < lastHealth)
        {
            StartCoroutine(HurtyCoroutine());
        }
        lastHealth = (float)playerController.Health.Value;
    }
    private IEnumerator HurtyCoroutine()
    {
        float duration = 0.2f;
        HurtIndicator.SetActive(true);

        yield return new WaitForSeconds(duration);

        HurtIndicator.SetActive(false);
    }
    void Start()
    {
        // initialize game music
        SoundMaster._instance.StartMusic("Game");
    }
    void Update()
    {
        UpdateHealthSlider();
        UpdateAbilitySlider();
        UpdateInteractSlider();
        UpdateGreenFlash();
        HurtyIndicator();
    }

    public void OnEscInput()
    {
        Debug.Log("OnEscInput");
        MenuIsShown = !MenuIsShown;
    }

    public void OnContinue()
    {
        MenuIsShown = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SetIsLocal(bool local)
    {
        foreach (GameObject obj in localActiveUi)
        {
            obj.SetActive(local);
        }
    }

    public void FlashGreen(float duration)
    {
        flashGreenStartTime = Time.time;
        flashGreenDuration = duration;
    }

    public void OnQuit()
    {
        LocalGameManager.Instance.LeaveLobby();
    }
}