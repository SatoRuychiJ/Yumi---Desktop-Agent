using LinePutScript;
using LinePutScript.Localization.WPF;
using Panuon.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using VPet_Simulator.Windows.Interface;

namespace VPet_Simulator.Windows
{
    /// <summary>
    /// Interaction logic for winInventory.xaml
    /// </summary>
    public partial class winInventory : WindowX
    {
        MainWindow mw;
        private TextBox _searchTextBox;
        private Run rTotalValue;
        private Item _detailItem;
        private int _detailCount = 1;

        /// <summary>
        /// Constructor, initializes the inventory window
        /// </summary>
        /// <param name="mw">Main window instance</param>
        public winInventory(MainWindow mw)
        {
            InitializeComponent();
            this.mw = mw;
            Loaded += winInventory_Loaded;
            for (int i = 1; i < Item.ItemTypes.Count; i++)
            {
                LsbCategory.Items.Add(new ListBoxItem() { Content = ("Item_" + Item.ItemTypes[i]).Translate() });
            }
            //mw.ItemsAdd(mw.Foods[2].Clone());
        }

        /// <summary>
        /// Show the inventory window
        /// </summary>
        public new void Show()
        {
            base.Show();
            // On first open the controls may not be fully loaded yet; delay the refresh to avoid "not loading on open, only loading after switching"
            Dispatcher.BeginInvoke(new Action(UpdateList), DispatcherPriority.Loaded);

        }

        private void winInventory_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateList();
        }

        /// <summary>
        /// Update the item list
        /// </summary>
        /// <remarks>
        /// Update the displayed item list based on search, category and sort rules, and calculate the total value
        /// </remarks>
        private void UpdateList()
        {
            if (mw == null) return;

            // All items (used for the "total value" stat at the top)
            IEnumerable<Item> items = mw.Items.Where(x => x.Visibility);

            // Search
            if (_searchTextBox != null && !string.IsNullOrWhiteSpace(_searchTextBox.Text))
            {
                items = items.Where(x => x.TranslateName.Contains(_searchTextBox.Text));
            }

            // Favorites
            if (_puswitch?.IsChecked == true)
            {
                items = items.Where(x => x.Star == true);
            }

            // Category
            // 0: All (show all items)
            if (LsbCategory.SelectedIndex != 0)
            {
                items = items.Where(x => x.ItemType == Item.ItemTypes[LsbCategory.SelectedIndex]);
            }

            // Sort
            bool asc = LsbSortAsc.SelectedIndex == 0;
            switch (LsbSortRule.SelectedIndex)
            {
                default:
                    break;
                case 1: // By name
                    items = asc ? items.OrderBy(x => x.Name) : items.OrderByDescending(x => x.Name);
                    break;
                case 2: // By count
                    items = asc ? items.OrderBy(x => x.Count) : items.OrderByDescending(x => x.Count);
                    break;
                case 3: // By price
                    items = asc ? items.OrderBy(x => x.Price) : items.OrderByDescending(x => x.Price);
                    break;
            }

            IcCommodity.ItemsSource = items;

            // Calculate the total value (always counts all items, unaffected by category)
            double totalValue = mw.Items.Sum(x => x.Price * x.Count);
            if (rTotalValue != null)
                rTotalValue.Text = totalValue.ToString("f2");

            if (!items.Any())
                TbNone.Visibility = Visibility.Visible;
            else
                TbNone.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Use an item
        /// </summary>
        private void UseItem(Item item, int count = 1)
        {
            if (item == null) return;
            while (count-- > 0)
                item.Use(mw);
            // No notification, refresh directly
            UpdateList();
        }

        /// <summary>
        /// Search button click handler
        /// </summary>
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            UpdateList();
        }

        /// <summary>
        /// Title search box loaded handler
        /// </summary>
        private void TbTitleSearch_Loaded(object sender, RoutedEventArgs e)
        {
            _searchTextBox = sender as TextBox;
        }

        /// <summary>
        /// Total value text loaded handler
        /// </summary>
        private void rTotalValue_Loaded(object sender, RoutedEventArgs e)
        {
            rTotalValue = sender as Run;
            var totalValue = mw.Items.Sum(x => x.Price * x.Count);
            rTotalValue.Text = totalValue.ToString("f2");
        }


        /// <summary>
        /// Sort rule selection changed handler
        /// </summary>
        private void LsbSortRule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded)
                UpdateList();
        }

        ///// <summary>
        ///// Window closing handler
        ///// </summary>
        //private void WindowX_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    IcCommodity.ItemsSource = null;
        //    HideDetail();
        //    Hide();
        //    e.Cancel = true;
        //}

        private void BtnHoverUse_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var btn = sender as Button;
            var item = btn?.DataContext as Item;
            if (item == null)
                return;
            UseItem(item);
        }
        private void BtnHoverView_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var btn = sender as Button;
            var item = btn?.DataContext as Item;
            if (item == null)
                return;
            DisplayDetail(item);
        }

        /// <summary>
        /// Item cell click handler
        /// </summary>
        private void CellRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var bdr = sender as Border;
            var item = bdr?.DataContext as Item;
            if (item == null)
                return;
            DisplayDetail(item);
        }

        /// <summary>
        /// Show the item detail panel
        /// </summary>
        /// <param name="item">The item to display</param>
        private void DisplayDetail(Item item)
        {
            _detailItem = item;
            _detailCount = 1;

            TextItemName.Text = item.TranslateName;
            ImageItemDetail.Source = item.ImageSource;
            TextItemPrice.Text = $"$ {item.Price:f2}";
            TextItemDesc.Text = item.Description ?? "";

            runMax.Text = item.Count.ToString();

            TbDetailCount.Text = _detailCount.ToString();
            TbtnStar.IsChecked = item.Star;
            IsMaskVisible = true;
            IsOverlayerVisible = true;

            if (item.IsSingle)
            {
                spUseEnter.Visibility = Visibility.Collapsed;
                BorderIsSingle.Visibility = Visibility.Visible;
            }
            else
            {
                spUseEnter.Visibility = Visibility.Visible;
                BorderIsSingle.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Hide the item detail panel
        /// </summary>
        private void HideDetail()
        {
            IsMaskVisible = false;
            IsOverlayerVisible = false;
        }

        /// <summary>
        /// Detail panel outside-area click handler
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        /// <remarks>
        /// Close the detail panel when clicking outside it
        /// </remarks>
        private void BorderOutDetail_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HideDetail();
        }

        /// <summary>
        /// Close detail button click handler
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void ButtonCloseDetail_Click(object sender, RoutedEventArgs e)
        {
            HideDetail();
        }

        /// <summary>
        /// Detail panel decrease-quantity button click handler
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void RbtnDetailDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (_detailItem == null)
                return;
            _detailCount = Math.Max(1, _detailCount - 1);
            TbDetailCount.Text = _detailCount.ToString();
        }

        /// <summary>
        /// Detail panel increase-quantity button click handler
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void RbtnDetailIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (_detailItem == null)
                return;
            _detailCount = Math.Min(Math.Max(1, _detailItem.Count), _detailCount + 1);
            TbDetailCount.Text = _detailCount.ToString();
        }

        /// <summary>
        /// Detail panel quantity textbox key handler
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        /// <remarks>
        /// Apply the entered quantity when Enter is pressed
        /// </remarks>
        private void TbDetailCount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyDetailCountFromText();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Detail panel quantity textbox lost-focus handler
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void TbDetailCount_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplyDetailCountFromText();
        }

        /// <summary>
        /// Apply the quantity setting from the textbox
        /// </summary>
        /// <remarks>
        /// Validate and clamp the entered quantity to the valid range (1 to the owned count)
        /// </remarks>
        private void ApplyDetailCountFromText()
        {
            if (_detailItem == null)
                return;
            if (!int.TryParse(TbDetailCount.Text?.Trim(), out var v))
                v = _detailCount;
            v = Math.Max(1, v);
            v = Math.Min(Math.Max(1, _detailItem.Count), v);
            _detailCount = v;
            TbDetailCount.Text = _detailCount.ToString();
        }

        /// <summary>
        /// Detail panel use button click handler
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void BtnDetailUse_Click(object sender, RoutedEventArgs e)
        {
            if (_detailItem == null)
                return;
            ApplyDetailCountFromText();
            UseItem(_detailItem, _detailCount);
            HideDetail();
        }

        private CheckBox _puswitch;
        private void Switch_Loaded(object sender, RoutedEventArgs e)
        {
            _puswitch = sender as CheckBox;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsLoaded)
                UpdateList();
        }

        private void TbtnStar_Checked(object sender, RoutedEventArgs e)
        {
            if (_detailItem != null)
                _detailItem.Star = TbtnStar.IsChecked == true;
        }
    }
}
