using LinePutScript;
using LinePutScript.Converter;
using System.Text;

namespace VPet_Simulator.Windows.Interface;

/// <summary>
/// Message transmitted in multiplayer mode
/// </summary>
public struct MPMessage
{
    /// <summary>
    /// Message type
    /// </summary>
    public enum MSGType
    {
        /// <summary>
        /// Usually an error or empty message
        /// </summary>
        Empty,
        /// <summary>
        /// Chat message (chat)
        /// </summary>
        Chat,
        /// <summary>
        /// Show animation (graphinfo)
        /// </summary>
        DispayGraph,
        /// <summary>
        /// Interaction (Interact)
        /// </summary>
        Interact,
        /// <summary>
        /// Feed (Feed)
        /// </summary>
        Feed,
    }
    /// <summary>
    /// Message type. MOD authors can pick any number not in MSGTYPE to avoid conflicts; negative numbers are supported
    /// </summary>
    [Line] public int Type { get; set; }

    /// <summary>
    /// Message content
    /// </summary>
    [Line] private string Content { get; set; }
    /// <summary>
    /// The target being acted on (used for showing animations)
    /// </summary>
    [Line] public ulong To { get; set; }

    public static byte[] ConverTo(MPMessage data) => Encoding.UTF8.GetBytes(LPSConvert.SerializeObject(data).ToString());
    public static MPMessage ConverTo(byte[] data) => LPSConvert.DeserializeObject<MPMessage>(new LPS(Encoding.UTF8.GetString(data)));
    /// <summary>
    /// Set the message content (class)
    /// </summary>
    public void SetContent(object content)
    {
        Content = LPSConvert.GetObjectString(content, convertNoneLineAttribute: true);
    }
    /// <summary>
    /// Get the message content (class)
    /// </summary>
    /// <typeparam name="T">Class type</typeparam>
    public T GetContent<T>()
    {
        return (T)LPSConvert.GetStringObject(Content, typeof(T), convertNoneLineAttribute: true);
    }
    /// <summary>
    /// Set the message content (string)
    /// </summary>
    public void SetContent(string content)
    {
        Content = content;
    }
    /// <summary>
    /// Get the message content (string)
    /// </summary>
    public string GetContent()
    {
        return Content;
    }
    /// <summary>
    /// Chat structure
    /// </summary>
    public struct Chat
    {
        /// <summary>
        /// Chat content
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Message type
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Private
            /// </summary>
            Private,
            /// <summary>
            /// Semi-public
            /// </summary>
            Internal,
            /// <summary>
            /// Public
            /// </summary>
            Public
        }
        /// <summary>
        /// Chat type
        /// </summary>
        public Type ChatType { get; set; }
        /// <summary>
        /// Sender name
        /// </summary>
        public string SendName { get; set; }
        /// <summary>
        /// Receiver name
        /// </summary>
        public string ToName { get; set; }
    }
    /// <summary>
    /// Interaction structure
    /// </summary>
    public struct Feed
    {
        /// <summary>
        /// Whether the other side has data calculation enabled (and has not lost the indicator)
        /// </summary>
        public bool EnableFunction { get; set; }
        /// <summary>
        /// Food/item
        /// </summary>
        [Line()]
        public Food Item { get; set; }
    }
    /// <summary>
    /// Interaction type
    /// </summary>
    public enum Interact
    {
        /// <summary>
        /// Pet the body
        /// </summary>
        TouchHead,
        /// <summary>
        /// Pat the head
        /// </summary>
        TouchBody,
        /// <summary>
        /// Pinch the face
        /// </summary>
        TouchPinch,
    }
}
