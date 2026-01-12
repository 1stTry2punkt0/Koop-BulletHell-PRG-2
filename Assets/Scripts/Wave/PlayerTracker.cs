using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerTracker
{
    // A list to keep track of all registered players
    private static readonly List<PlayerMovement> _players = new List<PlayerMovement>();

    // Public read-only access to the list of players
    public static IReadOnlyList<PlayerMovement> Players => _players;
    // Register a player to the tracker
    public static void RegisterPlayer(PlayerMovement player)
    {
        if (!_players.Contains(player))
        {
            _players.Add(player);
        }
    }
    // Unregister a player from the tracker
    public static void UnregisterPlayer(PlayerMovement player)
    {
            _players.Remove(player);
    }
}
