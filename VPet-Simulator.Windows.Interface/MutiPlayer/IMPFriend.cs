using LinePutScript;
using System.Windows.Media;
using VPet_Simulator.Core;
using static System.Windows.Forms.AxHost;
using static VPet_Simulator.Core.GraphInfo;
using static VPet_Simulator.Windows.Interface.MPMessage;

namespace VPet_Simulator.Windows.Interface;
/// <summary>
/// Interface for a friend's pet (graphics) module
/// </summary>
public interface IMPFriend
{
    /// <summary>
    /// Guest list id
    /// </summary>
    ulong LobbyID { get; }
    /// <summary>
    /// Friend id
    /// </summary>
    ulong FriendID { get; }

    /// <summary>
    ///  Stream: Returns true if this is the local user
    /// </summary>
    bool IsMe { get; }

    /// <summary>
    ///  Stream: Return true if this is a friend
    /// </summary>
    bool IsFriend { get; }
    /// <summary>
    /// Stream: Returns true if you have this user blocked
    /// </summary>
    bool IsBlocked { get; }

    /// <summary>
    /// Stream: Return true if this user is playing the game we're running
    /// </summary>
    bool IsPlayingThisGame { get; }

    /// <summary>
    /// Stream: Returns true if this friend is online
    /// </summary>
    bool IsOnline { get; }

    /// <summary>
    /// Stream:  Returns true if this friend is marked as away
    /// </summary>
    bool IsAway { get; }

    /// <summary>
    /// Stream: Returns true if this friend is marked as busy
    /// </summary>
    bool IsBusy { get; }

    /// <summary>
    /// Stream: Returns true if this friend is marked as snoozing
    /// </summary>
    bool IsSnoozing { get; }
    /// <summary>
    /// Stream: friend name
    /// </summary>
    string Name { get; }
    /// <summary>
    /// Stream: friend level
    /// </summary>
    int SteamLevel { get; }
    /// <summary>
    /// Stream: friend avatar (mind the UI thread)
    /// </summary>
    ImageSource Avatar { get; }

    /// <summary>
    /// Pet data core
    /// </summary>
    GameCore Core { get; }
    /// <summary>
    /// Image resource set
    /// </summary>
    ImageResources ImageSources { get; }
    /// <summary>
    /// Current pet graphic name
    /// </summary>
    string SetPetGraph { get; }
    /// <summary>
    /// Pet main component
    /// </summary>
    Main Main { get; }

    /// <summary>
    /// Intelligently show the subsequent transition animation
    /// </summary>
    void DisplayAuto(GraphInfo gi);

    /// <summary>
    /// Show the animation based on friend data
    /// </summary>
    bool DisplayGraph(GraphInfo gi);
    /// <summary>
    /// Show chat messages between friends
    /// </summary>
    /// <param name="msg">Chat content</param>
    void DisplayMessage(Chat msg);

    /// <summary>
    /// Determine whether busy (being picked up, etc.; cannot interact)
    /// </summary>
    /// <returns></returns>
    public bool InConvenience();

    /// <summary>
    /// Determine whether busy (being picked up, etc.; cannot interact)
    /// </summary>
    public static bool InConvenience(Main Main)
    {
        if (Main.DisplayType.Type == GraphType.StartUP || Main.DisplayType.Type == GraphType.Raised_Dynamic || Main.DisplayType.Type == GraphType.Raised_Static)
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// Show the eating (sandwich) animation
    /// </summary>
    /// <param name="graphName">Sandwich animation name</param>
    /// <param name="imageSource">The image sandwiched in the middle</param>
    void DisplayFoodAnimation(string graphName, ImageSource imageSource);
    /// <summary>
    /// Whether interaction is disallowed
    /// </summary>
    bool NOTouch { get; }
}
