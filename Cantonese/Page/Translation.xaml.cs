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
using Cantonese.Model;
using System.Net;
using System.Net.Http;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Windows.Media.SpeechRecognition;
using Cantonese.Voice;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.ApplicationModel;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace Cantonese
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Translation : Page
    {
        LanguageModel TranslationMode;
        bool VoiceRecordSym;

        //判断所选择的翻译前的语言和翻译后的语言
        public Translation()
        {
            this.InitializeComponent();
            TranslationMode = new LanguageModel();
            TranslationMode.From = "zh";
            TranslationMode.To = "yue";
            VoiceRecordSym = true;
        }
        
        //粤语与中文互换
        //调用百度翻译api
        private void TranslationButton_Click(object sender, RoutedEventArgs e)
        {
            HttpClient client = new HttpClient();
            if (!string.IsNullOrEmpty(Input.Text))
            {
                Output.Text = Voice.Translation.Translate(Input.Text, TranslationMode).Result;
            }
        }

        //粤语翻中文和中文翻粤语模式互换
        private void SwitchButton_Click(object sender, RoutedEventArgs e)
        {
            string temp = OriginalLanguage.Text;
            OriginalLanguage.Text = TargetLanguage.Text;
            TargetLanguage.Text = temp;
            TranslationMode.From = OriginalLanguage.Text == "普通话" ? "zh" : "yue";
            TranslationMode.To = TargetLanguage.Text == "普通话" ? "zh" : "yue";
            temp = Output.Text;
            Output.Text = Input.Text;
            Input.Text = temp;
        }

        //判断转语音按钮的可见性
        private void Output_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Output.Text != null && TranslationMode.To == "yue") CopyButton.Visibility = Visibility.Visible;
            else CopyButton.Visibility = Visibility.Collapsed;
        }

        //导航键
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Pronunciation), Output.Text);
        }

        //语音录入
        private MediaCapture _mediaCapture;
        private InMemoryRandomAccessStream _memoryBuffer = new InMemoryRandomAccessStream();
        public bool IsRecording { get; set; }
        private async void VoiceCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            string output;
            //开始录音
            if (VoiceRecordSym==true)
            {
                _memoryBuffer = new InMemoryRandomAccessStream();
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
                VoiceCaptureButton.Content = "语音识别";
                VoiceRecordSym = true;
                //转换InMemoryRandomAccessStream成Stream
                Stream tempStream = WindowsRuntimeStreamExtensions.AsStreamForRead(_memoryBuffer.GetInputStreamAt(0));
                using (var stream = new MemoryStream())
                {
                    tempStream.CopyTo(stream);
                    VoiceToText voiceToText = new VoiceToText();
                    //传入VoiceToText函数
                    output = voiceToText.ReadVoice(stream, TranslationMode.From).Result;
                }
                //tempStream.Position = 0;
                Input.Text = output;
            }
        }
    }
}
