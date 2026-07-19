using Steamworks;
using Steamworks.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VPet_Simulator.Windows;
/// <summary>
/// Interaction logic for MPUserControl.xaml
/// </summary>
public partial class MPUserControl : Border
{
    public Friend friend => mpf.friend;
    winMutiPlayer wmp;
    public MPFriends mpf;
    Lobby lb => mpf.lb;
    public MPUserControl(winMutiPlayer wmp, MPFriends mpf)
    {
        InitializeComponent();
        this.wmp = wmp;
        this.mpf = mpf;
        Task.Run(LoadInfo);
    }
    public void LoadInfo()
    {
        // Load the data passed in from the lobby
        while (!mpf.Loaded)
        {
            Thread.Sleep(500);
        }
        Dispatcher.Invoke(async () =>
        {
            rPetName.Text = mpf.Core.Save.Name;
            hostName.Text = friend.Name;
            var img = await friend.GetMediumAvatarAsync();
            uimg.Source = winMutiPlayer.ConvertToImageSource(img);
            info.Text = "Lv " + mpf.Core.Save.Level;
            if (lb.Owner.IsMe)
                Kick.Visibility = Visibility.Visible;
        });
    }

    private void btn_ReSetLocal(object sender, RoutedEventArgs e)
    {
        mpf.ReSetLocal();
    }


    private void Kick_Click(object sender, RoutedEventArgs e)
    {
        lb.SetData("kick", friend.Id.Value.ToString());
    }
}
