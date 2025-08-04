using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.Netcode;

public abstract class PlayerAbilties : NetworkBehaviour
{
    public float cooldown;
    public abstract void Ability();
    private bool usingAbility;
    public bool gunuse;
    public float CDtime;
    private PlayerController playerController;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        usingAbility = false;
        gunuse = false;
    }

    public void OnEnable()
    {
        playerController = gameObject.GetComponent<PlayerController>();
    }

    public void OnAttack()
    {
        if (playerController == null)
        {
            gameObject.TryGetComponent(out PlayerController p);
            if (p == null)
            {
                Debug.LogWarning("PlayerAbilities could not find a PlayerController");
            }
            else
            {
                Debug.LogWarning("This shouldn't happen! playerController was null, but it was set properly OnAttack.");
                playerController = p;
            }
        }
        if (playerController.stunned) return;
        if (!enabled || !IsOwner) return;
        if (MainCanvasController.Instance is GameCanvasController gameCanvasController)
        {
            if (gameCanvasController.MenuIsShown) { return; }
        }
        else
        {
            Debug.LogWarning("PlayerAbilities could no find a valid GameCanvasController!");
        }

        if (!usingAbility || gunuse)
        {
            StartCoroutine(AttackSequence());
        }
    }
    public void Update()
    {
        if (CDtime < cooldown)
        { 
            CDtime += Time.deltaTime;
        }
    }
    IEnumerator AttackSequence()
    {
        CDtime = 0;
        usingAbility = true;
        Ability();
        yield return new WaitForSeconds(cooldown);
        usingAbility = false;
    }
}
