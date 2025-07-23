using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Server;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static readonly ConcurrentQueue<Action> _mainThreadActions = new ConcurrentQueue<Action>();

    public static void RunOnMainThread(Action action)
    {
        if (action == null) return;
        _mainThreadActions.Enqueue(action);
    }

    void Update()
    {
        Debug.Log("Update!");
        while (_mainThreadActions.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }

    public static GameManager Manager
    {
        get
        {
            if (manager == null)
            {
                manager = FindObjectOfType<GameManager>();
            }
            return manager;
        }
    }
    private static GameManager manager = null;
    [SerializeField] public ChipPile centerBankPile;


    [SerializeField] MBPlayer[] players;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log($"Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
        Program.Main();
    }


    public void SetPlayers(List<IBot> bots)
    {
        for (int i = 0; i < 6; i++)
        {
            players[i].SetPlayer(bots[i]);
        }
    }

    public MBPlayer GetPlayerByBotId(int botId)
    {
        return players.Where(player => player.GetBotId() == botId).ToList()[0];
    }
}
