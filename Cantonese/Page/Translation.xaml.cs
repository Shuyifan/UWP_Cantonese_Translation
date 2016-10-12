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
        TranslationResult TranslationInfo;
        string AppID = "20160607000022870";
        string AppKey = "Lki1IKW05LpLiuy3qNs8";
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
        private async void TranslationButton_Click(object sender, RoutedEventArgs e)
        {
            TranslationInfo = new TranslationResult();
            string url = "";
            HttpClient client = new HttpClient();
            TranslationInfo.src = Input.Text;
            if (!string.IsNullOrEmpty(Input.Text))
            {
                Random ran = new Random();
                int temp = ran.Next();
                string Enter = Input.Text.Replace("\r\n", "###换行符###");
                string tempString1 = get_uft8(Enter), tempString2 = temp.ToString();
                string sign = AppID + tempString1 + tempString2 + AppKey;
                sign = ComputeMD5(sign);
                url = string.Format("http://api.fanyi.baidu.com/api/trans/vip/translate?q={0}&from={1}&to={2}&appid={3}&salt={4}&sign={5}"
                    , UrlEncode(tempString1)
                    , TranslationMode.From
                    , TranslationMode.To
                    , AppID
                    , tempString2
                    , sign);
                var response = await client.GetByteArrayAsync(url);
                string result = Encoding.UTF8.GetString(response);
                StringReader sr = new StringReader(result);
                JsonTextReader jsonReader = new JsonTextReader(sr);
                JsonSerializer serializer = new JsonSerializer();
                var r = serializer.Deserialize<LanguageModel>(jsonReader);
                Output.Text = r.Trans_result[0].dst.Replace("###换行符###", "\r\n").Replace("###换走符###", "\r\n");
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

        //MD5加密
        private static string ComputeMD5(string str)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            var hashed = alg.HashData(buff);
            var res = CryptographicBuffer.EncodeToHexString(hashed);
            return res;
        }

        //对文本进行UTF8编码
        private static string get_uft8(string unicodeString)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            Byte[] encodedBytes = utf8.GetBytes(unicodeString);
            String decodedString = utf8.GetString(encodedBytes);
            return decodedString;
        }

        //URI解码
        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString());
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
