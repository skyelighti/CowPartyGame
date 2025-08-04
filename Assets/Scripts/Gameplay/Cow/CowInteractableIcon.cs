using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class CowInteractableIcon : MonoBehaviour, IInteractableIcon
{
    private CinemachineBrain brain;

    [SerializeField] private Sprite arrow;
    [SerializeField] private Sprite ufo;
    private bool showingUfo;
    [SerializeField] private Sprite cancel;

    private Image image;

    private void Start()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();
        image = GetComponent<Image>();
    }

    private void Update()
    {
        Vector3 brainPos = brain.State.GetFinalPosition();
        Vector3 pos = transform.position;
        brainPos.y = pos.y;
        image.transform.LookAt(brainPos);
    }

    public void OnPlayerEnterRange(LocalPlayerController localPlayerController)
    {
        image.enabled = true;
        image.sprite = arrow;
        CancelInvoke();
    }

    public void OnPlayerExitRange(LocalPlayerController localPlayerController)
    {
        Invoke(nameof(Hide), 1f);
        if (showingUfo)
        {
            image.sprite = cancel;
        }
    }

    public void OnInteract(LocalPlayerController localPlayerController)
    {
        image.enabled = true;
        image.sprite = arrow;
    }

    public void OnStopInteract(LocalPlayerController localPlayerController)
    {

    }

    public void ShowUfo()
    {
        showingUfo = true;
        image.enabled = true;
        image.sprite = ufo;
    }

    public void Hide()
    {
        showingUfo = false;
        image.enabled = false;
    }
}
