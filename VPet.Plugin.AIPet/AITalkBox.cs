using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.AIPet
{
    /// <summary>
    /// AI chat box: user input -> AI brain -> streamed speech
    /// </summary>
    public class AITalkBox : TalkBox
    {
        private readonly AIPetPlugin plugin;

        public AITalkBox(AIPetPlugin plugin) : base(plugin)
        {
            this.plugin = plugin;
        }

        public override string APIName => "AIDeskPet";

        /// <summary>
        /// User sends a message (the base class already calls this on a background thread)
        /// </summary>
        public override void Responded(string text)
        {
            plugin.Controller?.HandleUserMessage(text);
        }

        public override void Setting()
        {
            plugin.Setting();
        }
    }
}
