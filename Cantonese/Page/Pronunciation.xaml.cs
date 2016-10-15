using Cantonese.Voice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace Cantonese
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Pronunciation : Page
    {
        Dictionary<string, string> Dictionary;

        public Pronunciation()
        {
            this.InitializeComponent();
            Init();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter!=null) Input.Text = e.Parameter.ToString();
        }

        private void Init()
        {
            Dictionary = new Dictionary<string, string>();
            //载入字典
            string path = ".\\Assets\\Dictionary.txt";
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader st = new StreamReader(fs, Encoding.UTF8);
            string line;
            //将txt文件载入Dictionary类
            while ((line = st.ReadLine()) != null)
            {
                char Splitor = '\t';
                string[] temp = line.Split(Splitor);
                if(Dictionary.ContainsKey(temp[0])) Dictionary[temp[0]] += ("/" + temp[1]);
                else Dictionary.Add(temp[0], temp[1]);
            }
        }

        //对文字根据字典进行替换
        private void TranslationButton_Click(object sender, RoutedEventArgs e)
        {
            string OutputText = "  ";
            for (int i = 0; i < Input.Text.Length; i++)
            {
                if((int)Input.Text[i] >= 19968&&(int)Input.Text[i]<=40869)
                {
                    if(Dictionary.ContainsKey(Input.Text[i].ToString()))
                    {
                        OutputText += (Dictionary[Input.Text[i].ToString()] + "  ");
                    }
                    else
                    {
                        OutputText += Input.Text[i] + "  ";
                    }
                }
                else
                {
                    OutputText += Input.Text[i];
                }
            }
            Output.Text = OutputText;
        }

        //粤语发音
        private async void Voice_Click(object sender, RoutedEventArgs e)
        {
            TextToVoice temp = new TextToVoice();
            Stream voice = await temp.ReadText(Input.Text);
            InMemoryRandomAccessStream ras = new InMemoryRandomAccessStream();
            await voice.CopyToAsync(ras.AsStreamForWrite());
            await ras.FlushAsync();
            ras.Seek(0);
            MyMediaPlayer.SetSource(ras, "");
        }

        private void VoiceCaptureButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
