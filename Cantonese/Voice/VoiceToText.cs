using Cantonese.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cantonese.Voice
{
    class VoiceToText
    {
        private static readonly string AccessUri = "https://oxford-speech.cloudapp.net/token/issueToken";
        private string ClientID = "5031686d942f4532b0109975e647fb7f";
        private string ClientSecter = "5031686d942f4532b0109975e647fb7f";
        private AccessTokenInfo token;
        private Timer accessTokenRenewer;

        private const int RefreshTokenDuration = 9;

        //Access Token获取授权
        private void Authentication()
        {

            token = RequestTokenAsync().Result;

            accessTokenRenewer = new Timer(new TimerCallback(OnTokenExpiredCallback),
                               this,
                               TimeSpan.FromMinutes(RefreshTokenDuration),
                               TimeSpan.FromMilliseconds(-1));

        }

        //发送授权uri，获取response
        private async Task<AccessTokenInfo> RequestTokenAsync()
        {
            Uri reqUri = new Uri(AccessUri);
            AccessTokenInfo tokendt = null;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Dictionary<string, string> formdata = new Dictionary<string, string>
                    {
                        {"grant_type","client_credentials" },
                        {"client_id",ClientID },
                        {"client_secret",ClientSecter },
                        {"scope","https://speech.platform.bing.com" }
                    };
                    FormUrlEncodedContent content = new FormUrlEncodedContent(formdata);
                    HttpResponseMessage response = null;
                    response = await client.PostAsync(reqUri, content).ConfigureAwait(false);
                    if (response != null && response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream stream = await response.Content.ReadAsStreamAsync())
                        {
                            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AccessTokenInfo));
                            tokendt = (AccessTokenInfo)serializer.ReadObject(stream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.GetType();
            }
            return tokendt;
        }

        //accesstoken时限为10分钟，过一阵子进行刷新
        private void RenewAccessToken()
        {
            AccessTokenInfo newAccessToken = RequestTokenAsync().Result;
            //swap the new token with old one
            //Note: the swap is thread unsafe
            this.token = newAccessToken;
        }

        //刷新的调回方法
        private void OnTokenExpiredCallback(object stateInfo)
        {
            try
            {
                RenewAccessToken();
            }
            catch (Exception ex)
            {
                ex.GetType();
            }
            finally
            {
                try
                {
                    accessTokenRenewer.Change(TimeSpan.FromMinutes(RefreshTokenDuration), TimeSpan.FromMilliseconds(-1));
                }
                catch (Exception ex)
                {
                    ex.GetType();
                    //Console.WriteLine(string.Format("Failed to reschedule the timer to renew access token. Details: {0}", ex.Message));
                }   
            }
        }

        //语音识别
        public async Task<string> ReadVoice(Stream input, string InputLanguage)
        {
            string headerValue;
            Authentication();//获取accesstoken
            string requestUri = "https://speech.platform.bing.com/recognize";
            requestUri += @"?scenarios=smd";                                  // websearch is the other main option.
            requestUri += @"&appid=D4D52672-91D7-4C74-8AD8-42B1D98141A5";     // You must use this ID.
            if(InputLanguage=="zh") requestUri += @"&locale=zh-CN";  // We support several other languages.  Refer to README file.
            else requestUri += @"&locale=zh-HK";
            requestUri += @"&device.os=wp7";
            requestUri += @"&version=3.0";
            requestUri += @"&format=json";
            requestUri += @"&instanceid=565D69FF-E928-4B7E-87DA-9A750B96D9E3";
            requestUri += @"&requestid=" + Guid.NewGuid().ToString();

            //string host = @"speech.platform.bing.com";
            string contentType = @"audio/wav; codec=""audio/pcm""; samplerate=16000";

            //string audioFile = input;
            string responseString="";
            //FileStream fs = null;

            try
            {
                /*
                 * Create a header with the access_token property of the returned token
                 */
                headerValue = "Bearer " + token.access_token;

                HttpWebRequest request = null;
                request = (HttpWebRequest)HttpWebRequest.Create(requestUri);
                //request.SendChunked = true;
                request.Accept = @"application/json;text/xml";
                request.Method = "POST";
                //request.ProtocolVersion = HttpVersion.Version11;
                //request.Host = host;
                request.ContentType = contentType;
                request.Headers["Authorization"] = headerValue;
                //获取流数据
                using (Stream stream = input)
                {

                    /*
                     * Open a request stream and write 1024 byte chunks in the stream one at a time.
                     */
                    byte[] buffer = null;
                    int bytesRead = 0;
                    using (Stream requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
                    {
                        /*
                         * Read 1024 raw bytes from the input audio file.
                         */
                        buffer = new Byte[checked((uint)Math.Min(1024, (int)stream.Length))];
                        stream.Position = 0;
                        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            requestStream.Write(buffer, 0, bytesRead);
                        }

                        // Flush
                        requestStream.Flush();
                    }

                    /*
                     * Get the response from the service.
                     */
                    //Console.WriteLine("Response:");
                    using (WebResponse response = await request.GetResponseAsync().ConfigureAwait(false))
                    {
                        //Console.WriteLine(((HttpWebResponse)response).StatusCode);

                        using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        {
                            responseString = sr.ReadToEnd();
                        }

                        //Console.WriteLine(responseString);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.GetType();
            }
            //jason反解析
            StringReader steamreader = new StringReader(responseString);
            JsonTextReader jsonReader = new JsonTextReader(steamreader);
            JsonSerializer serializer = new JsonSerializer();
            var r = serializer.Deserialize<Result>(jsonReader);
            return r.results[0].lexical;
        }

    }
}
