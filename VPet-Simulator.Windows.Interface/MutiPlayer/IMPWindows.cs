using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace VPet_Simulator.Windows.Interface;

/// <summary>
/// Multiplayer window interface (guest list)
/// </summary>
public interface IMPWindows
{
    /// <summary>
    /// Guest list id
    /// </summary>
    ulong LobbyID { get; }
    /// <summary>
    /// All friends (excluding yourself)
    /// </summary>
    IEnumerable<IMPFriend> Friends { get; }
    /// <summary>
    /// Convert yourself into a friend object for easier batch processing
    /// </summary>
    IMPFriend SelftoIMPFriend();
    /// <summary>
    /// Host SteamID
    /// </summary>
    ulong HostID { get; }
    /// <summary>
    /// Whether the current player is the host
    /// </summary>
    bool IsHost { get; }

    /// <summary>
    /// Event: member left
    /// </summary>
    event Action<ulong> OnMemberLeave;
    /// <summary>
    /// Event: member joined
    /// </summary>
    event Action<ulong> OnMemberJoined;
    /// <summary>
    /// Send a message (data packet) to a specific friend
    /// </summary>
    /// <param name="friendid">Friend id</param>
    /// <param name="msg">Message content (data packet)</param>
    bool SendMessage(ulong friendid, MPMessage msg);

    /// <summary>
    /// Send a message to everyone
    /// </summary>
    void SendMessageALL(MPMessage msg);

    /// <summary>
    /// Send a log message
    /// </summary>
    /// <param name="message">Log</param>
    void Log(string message);

    /// <summary>
    /// Received message log: sender id, message content
    /// </summary>
    event Action<ulong, MPMessage> ReceivedMessage;
    /// <summary>
    /// Event: guest list ended, window closed
    /// </summary>
    event Action ClosingMutiPlayer;

    /// <summary>
    /// Whether a game (from another mod) is currently running, to avoid conflicts from multiple games at once
    /// When your game starts, set this to true, and set it back to false when the game ends
    /// </summary>
    bool IsGameRunning { get; set; }
    /// <summary>
    /// Get the guest list menu bar; you can insert your own menu
    /// </summary>
    TabControl TabControl { get; }
    /// <summary>
    /// Whether joinable
    /// </summary>
    bool Joinable { get; }
}
