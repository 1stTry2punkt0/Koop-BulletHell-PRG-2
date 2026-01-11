using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerTracker
{
    private static readonly List<PlayerMovement> _players = new List<PlayerMovement>();

    public static IReadOnlyList<PlayerMovement> Players => _players;
    public static void RegisterPlayer(PlayerMovement player)
    {
        if (!_players.Contains(player))
        {
            _players.Add(player);
        }
    }
    public static void UnregisterPlayer(PlayerMovement player)
    {
            _players.Remove(player);
    }
}
