using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cantonese.Model;
using System.Net.Http;
using Windows.Security.Cryptography.Core;
using System.IO;
using Newtonsoft.Json;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;

namespace Cantonese.Voice
{
    class Translation
    {
        private static TranslationResult TranslationInfo;
        private static string AppID = "20160607000022870";
        private static string AppKey = "Lki1IKW05LpLiuy3qNs8";

        public static async Task<string> Translate(string InputText, LanguageModel languageModel)
        {
            TranslationInfo = new TranslationResult();
            string url = "";
            HttpClient client = new HttpClient();

            Random ran = new Random();
            int temp = ran.Next();
            InputText = InputText.Replace("\r\n", "###换行符###");
            string tempString1 = get_uft8(InputText), tempString2 = temp.ToString();
            string sign = AppID + tempString1 + tempString2 + AppKey;
            sign = ComputeMD5(sign);

            url = string.Format("http://api.fanyi.baidu.com/api/trans/vip/translate?q={0}&from={1}&to={2}&appid={3}&salt={4}&sign={5}"
                                , UrlEncode(tempString1)
                                , languageModel.From
                                , languageModel.To
                                , AppID
                                , tempString2
                                , sign);
            var response = await client.GetByteArrayAsync(url).ConfigureAwait(false);
            string result = Encoding.UTF8.GetString(response);
            StringReader sr = new StringReader(result);
            JsonTextReader jsonReader = new JsonTextReader(sr);
            JsonSerializer serializer = new JsonSerializer();
            var r = serializer.Deserialize<LanguageModel>(jsonReader);

            return r.Trans_result[0].dst.Replace("###换行符###", "\r\n").Replace("###换走符###", "\r\n");
        }

        //对文本进行UTF8编码
        private static string get_uft8(string unicodeString)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            Byte[] encodedBytes = utf8.GetBytes(unicodeString);
            String decodedString = utf8.GetString(encodedBytes);
            return decodedString;
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

    }
}
