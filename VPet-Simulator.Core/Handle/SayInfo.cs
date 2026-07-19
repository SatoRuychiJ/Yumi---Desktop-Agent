using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VPet_Simulator.Core
{
    /// <summary>
    /// Base class added to support streaming speech
    /// </summary>
    public abstract class SayInfo
    {
        public SayInfo()
        {

        }
        /* --------- Message info -----------*/
        /// <summary>
        /// Image name
        /// </summary>
        public string GraphName;
        /// <summary>
        /// Speech description
        /// </summary>
        public string Desc;
        /// <summary>
        /// Message content
        /// </summary>
        public UIElement MsgContent;
        /// <summary>
        /// Whether to force display of the image
        /// </summary>
        public bool Force = false;
        /// <summary>
        /// Whether the voice has already been played
        /// </summary>
        public bool IsGenVoice = false;
        /// <summary>
        /// Get the speech text (waits for completion if streaming)
        /// </summary>
        public abstract Task<string> GetSayText();
    }
    /// <summary>
    /// Speech info class (the original SayInfo)
    /// </summary>
    public class SayInfoWithOutStream : SayInfo
    {
        /// <summary>
        /// Speech info
        /// </summary>
        /// <param name="text">Speech text</param>
        /// <param name="graphname">Image name</param>
        /// <param name="desc">Description</param>
        /// <param name="force">Force display of the image</param>
        public SayInfoWithOutStream(string text, string graphname = null, bool force = false, string desc = null)
        {
            Text = text;
            GraphName = graphname;
            Force = force;
            Desc = desc;
        }

        /// <summary>
        /// Speech info class
        /// </summary>
        /// <param name="text">Speech text</param>
        /// <param name="graphname">Image name</param>
        /// <param name="msgcontent">Message content</param>
        /// <param name="force">Force display of the image</param>
        public SayInfoWithOutStream(string text, UIElement msgcontent, string graphname = null, bool force = false)
        {
            Text = text;
            GraphName = graphname;
            MsgContent = msgcontent;
            Force = force;
        }
        /// <summary>
        /// Speech info class
        /// </summary>
        public SayInfoWithOutStream() { }
        /// <summary>
        /// Speech text
        /// </summary>
        public string Text;
        /// <summary>
        /// Get speech text interface implementation; simply returns Text
        /// </summary>
        public override Task<string> GetSayText()
        {
            return Task.FromResult(Text);
        }
    }
    /// <summary>
    /// Speech info class (SayInfo with streaming)
    /// </summary>
    public class SayInfoWithStream : SayInfo
    {
        /// <summary>
        /// Speech info class
        /// </summary>
        public SayInfoWithStream()
        {
        }
        /// <summary>
        /// Speech info class
        /// </summary>
        /// <param name="graphname">Image name</param>
        /// <param name="desc">Description</param>
        /// <param name="force">Force display of the image</param>
        public SayInfoWithStream(string graphname, bool force = false, string desc = null)
        {
            GraphName = graphname;
            Force = force;
            Desc = desc;
        }

        /// <summary>
        /// Speech info class
        /// </summary>
        /// <param name="graphname">Image name</param>
        /// <param name="msgcontent">Message content</param>
        /// <param name="force">Force display of the image</param>
        public SayInfoWithStream(UIElement msgcontent, string graphname = null, bool force = false)
        {
            GraphName = graphname;
            MsgContent = msgcontent;
            Force = force;
        }

        /// <summary>
        /// Speech content update event
        /// </summary>
        public event Action<(string fullText, string changedText)> Event_Update;
        /// <summary>
        /// Generation-finished event; string is the full generated text
        /// </summary>
        public event Action<string> Event_Finish;
        /// <summary>
        /// Current dialogue content
        /// </summary>
        public StringBuilder CurrentText = new StringBuilder();
        /// <summary>
        /// Whether generation is finished
        /// </summary>
        public bool IsFinishGen = false;

        /// <summary>
        /// Replace the entire current dialogue content with the specified text
        /// </summary>
        /// <param name="fullText">The replacement text</param>
        public void UpdateAllText(string fullText)
        {
            CurrentText = new StringBuilder(fullText);
            Event_Update?.Invoke((fullText, fullText));
        }

        /// <summary>
        /// Append to the current dialogue content
        /// </summary>
        /// <param name="text">Content to append</param>
        public void UpdateText(string text)
        {
            CurrentText.Append(text);
            Event_Update?.Invoke((CurrentText.ToString(), text));
        }

        /// <summary>
        /// Called when finished
        /// </summary>
        public void FinishGenerate()
        {
            if (IsFinishGen)
                return;
            IsFinishGen = true;
            Event_Finish?.Invoke(CurrentText.ToString());
        }

        /// <summary>
        /// Convert the current dialogue content to a non-streaming SayInfo (waits until finished)
        /// </summary>
        public async Task<SayInfoWithOutStream> ToNoneStream()
        {
            while (!IsFinishGen)
            {
                await Task.Delay(10);
            }
            return new SayInfoWithOutStream()
            {
                GraphName = GraphName,
                Force = Force,
                Desc = Desc,
                MsgContent = MsgContent,
                Text = CurrentText.ToString()
            };
        }
        /// <summary>
        /// Get the speech text (waits for completion when streaming)
        /// </summary>
        public override async Task<string> GetSayText()
        {
            while (!IsFinishGen)
            {
                await Task.Delay(10);
            }
            return CurrentText.ToString();
        }
    }
}
