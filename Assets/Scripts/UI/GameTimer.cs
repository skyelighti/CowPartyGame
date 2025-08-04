using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;
using FMODUnity;

public class GameTimer : NetworkBehaviour
{
    public TMP_Text timerText;
    private UIPulser uiPulser;
    // private NetworkVariable<float> timerStartTime = new(float.MaxValue);
    [SerializeField] private float timerDuration;
    private float TimerValue => timerDuration - (Time.time - levelStartTime);
    private float levelStartTime;

    private int lastPulse;

    public override void OnNetworkSpawn()
    {
        levelStartTime = Time.time;
    }

    void Start()
    {
        uiPulser = timerText.transform.GetComponent<UIPulser>();
    }

    void Update()
    {
        UpdateCowCount();
        if (IsServer)
        {
            if (TimerValue < 0)
            {
                NetworkedGameplayManager.Instance.OnTimerZero();
            }
        }
    }
    
    void FixedUpdate()
    {
        if ((int)TimerValue != lastPulse)
        {
            lastPulse = (int)TimerValue;
            if (TimerValue <= 10)
            {
                uiPulser.PulseUI(2f, .1f);
                SoundMaster._instance.PlayCountdown();//does not repeat if already playing
            }
            else if (TimerValue <= 30)
            {
                uiPulser.PulseUI(1.5f, .3f);
            }
            else if (TimerValue <= 60)
            {
                uiPulser.PulseUI(1.1f, .5f);
            }
        }

        string minutes = (lastPulse / 60).ToString();
        string seconds = (lastPulse % 60).ToString();
        if (seconds.Length < 2)
        {
            seconds = $"0{seconds}";
        }

        timerText.text = $"{minutes}:{seconds}";
    }


    [SerializeField] private TMP_Text Count;
    ulong cows;
    ulong cowsAtGameStart;
    ulong winconditionPercentageCows;
    void UpdateCowCount()
    {
        if (IsServer)
        {
            cows = (ulong)CowManager.Instance.cowsRemaining;
            cowsAtGameStart = (ulong)CowManager.Instance.cowsAtGameStart;
            winconditionPercentageCows = (ulong)CowManager.Instance.winconditionPercentageCows;
            UpdateCowCntRpc(cows, cowsAtGameStart, winconditionPercentageCows);
        }
    }

    [Rpc(SendTo.Everyone)]
    void UpdateCowCntRpc(ulong cowc, ulong cowsAtGameStart, ulong winconditionPercentageCows)
    {
        Count.text = (cowsAtGameStart - cowc).ToString() + "/" + ((int)(cowsAtGameStart*(float)(winconditionPercentageCows/100f))).ToString();
        for (int i = 0; i < (int)cowc; i++) {
            Vector3 iconpos = new Vector3(0,0 ,0 );
            //Instantiate();
        }
    }
}
