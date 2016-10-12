using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace Cantonese
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            IconsListBox.SelectedItem = Translation;
            MainWindowsFrame.Navigate(typeof(Translation));
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void IconsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //导航页面
            if (Translation.IsSelected) MainWindowsFrame.Navigate(typeof(Translation));
            if (Pronunciation.IsSelected) MainWindowsFrame.Navigate(typeof(Pronunciation));
            if (Instruction.IsSelected) MainWindowsFrame.Navigate(typeof(Instruction));
        }

        private void MainWindowsFrame_Navigated(object sender, NavigationEventArgs e)
        {
            //导航栏选择
            if (e.Content.ToString() == "Cantonese.Translation" && (!Translation.IsSelected))
                IconsListBox.SelectedItem = Translation;
            if (e.Content.ToString() == "Cantonese.Pronunciation" && (!Pronunciation.IsSelected))
                IconsListBox.SelectedItem = Pronunciation;
            
        }
    }
}
