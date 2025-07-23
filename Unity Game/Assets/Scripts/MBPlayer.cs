using System;
using Server;
using TMPro;
using UnityEngine;

public class MBPlayer : MonoBehaviour
{
    [SerializeField] CardLayout cardLayout;
    [SerializeField] ChipPile bankPile;
    [SerializeField] ChipPile potPile;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] string playerName;
    private int botId;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cardLayout = GetComponentInChildren<CardLayout>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        text.text = playerName;
        cardLayout = transform.Find("Player Card Layout").GetComponent<CardLayout>();
        bankPile = transform.Find("Bank Chip PIle").GetComponent<ChipPile>();
        potPile = transform.Find("Pot Chip PIle").GetComponent<ChipPile>();
        text = transform.Find("Canvas").Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetPlayer(IBot bot)
    {
        // set pot state
        botId = bot.ID;
        playerName = bot.Name;
        text.text = playerName;
        potPile.SetRawAmount(0);
        bankPile.SetRawAmount(bot.Bank);
    }

    public int GetBotId()
    {
        return botId;
    }

    public void Bet(int amount)
    {
        bankPile.SendTo(amount, potPile);
    }

    public void PushChips(int amount)
    {
        potPile.SendTo(amount, GameManager.Manager.centerBankPile);
    }

    public void WinPot(int amount)
    {
        GameManager.Manager.centerBankPile.SendTo(amount, bankPile);
    }
}
