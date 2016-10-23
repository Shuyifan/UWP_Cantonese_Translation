using Cantonese.Voice;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
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
        bool VoiceRecordSym;

        public Pronunciation()
        {
            this.InitializeComponent();
            Init();
            VoiceRecordSym = true;
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

        //语音录入
        private MediaCapture _mediaCapture;
        private InMemoryRandomAccessStream _memoryBuffer = new InMemoryRandomAccessStream();
        public bool IsRecording { get; set; }
        private async void VoiceCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            string output;
            //开始录音
            if (VoiceRecordSym == true)
            {
                _memoryBuffer = new InMemoryRandomAccessStream();
                VoiceCaptureButton.FontFamily = new FontFamily("Segoe UI");
                VoiceCaptureButton.Content = "停止录音";
                VoiceRecordSym = false;
                if (IsRecording)
                {
                    throw new InvalidOperationException("Recording already in progress!");
                }
                MediaCaptureInitializationSettings settings =
                  new MediaCaptureInitializationSettings
                  {
                      StreamingCaptureMode = StreamingCaptureMode.Audio
                  };
                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync(settings);
                //将录音文件存入_memoryBuffer里面
                await _mediaCapture.StartRecordToStreamAsync(MediaEncodingProfile.CreateWav(AudioEncodingQuality.Auto), _memoryBuffer);
                IsRecording = true;
            }
            //停止录音
            else
            {
                await _mediaCapture.StopRecordAsync();
                IsRecording = false;
                VoiceCaptureButton.FontFamily = new FontFamily("Segoe MDL2 Assets");
                VoiceCaptureButton.Content = "\xE1D6";
                VoiceRecordSym = true;
                progessRing.IsActive = true;
                Input.IsReadOnly = true;
                //转换InMemoryRandomAccessStream成Stream
                Stream tempStream = WindowsRuntimeStreamExtensions.AsStreamForRead(_memoryBuffer.GetInputStreamAt(0));
                using (var stream = new MemoryStream())
                {
                    tempStream.CopyTo(stream);
                    VoiceToText voiceToText = new VoiceToText();
                    //传入VoiceToText函数
                    output = await voiceToText.ReadVoice(stream, "yue");
                }
                //tempStream.Position = 0;
                progessRing.IsActive = false;
                Input.IsReadOnly = false;
                Input.Text += output;
            }
        }
    }
}
