using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DnspodUpdate
{
    public static class Utils
    {
        public static string[] split(this string s, string sp)
        {
            return System.Text.RegularExpressions.Regex.Split(s, sp);
        }
        public static string GetVal(this string s, string begin, string end)
        {

            return s.split(begin)[1].split(end)[0];

        }
        public static string[] GetVals(this string s, string begin, string end)
        {
            int i = 0;
            List<string> tmp = new List<string>();
            foreach (string item in s.split(begin))
            {
                i++;
                if (i == 1) { continue; }
                if (item.split(end).Count() > 0)
                    tmp.Add(item.split(end)[0]);
            }
            return tmp.ToArray();
        }


        public static Color ToColor(string color)
        {

            int red, green, blue = 0;
            char[] rgb;
            color = color.TrimStart('#');
            color = Regex.Replace(color.ToLower(), "[g-zG-Z]", "");
            switch (color.Length)
            {
                case 3:
                    rgb = color.ToCharArray();
                    red = Convert.ToInt32(rgb[0].ToString() + rgb[0].ToString(), 16);
                    green = Convert.ToInt32(rgb[1].ToString() + rgb[1].ToString(), 16);
                    blue = Convert.ToInt32(rgb[2].ToString() + rgb[2].ToString(), 16);
                    return Color.FromArgb(red, green, blue);
                case 6:
                    rgb = color.ToCharArray();
                    red = Convert.ToInt32(rgb[0].ToString() + rgb[1].ToString(), 16);
                    green = Convert.ToInt32(rgb[2].ToString() + rgb[3].ToString(), 16);
                    blue = Convert.ToInt32(rgb[4].ToString() + rgb[5].ToString(), 16);
                    return Color.FromArgb(red, green, blue);
                default:
                    return Color.FromName(color);

            }
        }
    }
    public class EasyHttpClient
    {

        /// <summary>
        /// 需要访问的网址
        /// </summary>
        public string Url { get; set; }


        private string _userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";
        /// <summary>
        /// 访问UA头
        /// </summary>
        public string UserAgent { get => _userAgent; set => _userAgent = value; }


        /// <summary>
        /// 来路地址
        /// </summary>
        public string Referer { get; set; }

        private Encoding _encode = Encoding.UTF8;
        public Encoding Encode { get => _encode; set => _encode = value; }


        /// <summary>
        /// 自动继承Cookies
        /// </summary>
        public bool AutoCookie { get; set; } = true;
        /// <summary>
        /// 请求时的Cookie
        /// </summary>
        public string Cookie { get; set; }

        public CookieContainer Cookies { get; set; } = new CookieContainer();

        string _contentType = "text/html";
        /// <summary>
        /// ContentType属性指定响应的 HTTP内容类型。如果未指定 ContentType，默认为TEXT/HTML,在Post类型中，强制为application/x-www-form-urlencoded; charset=UTF-8
        /// </summary>
        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }
        /// <summary>
        /// 代理Proxy 服务器用户名
        /// </summary>
        public string ProxyUserName { get; set; }
        /// <summary>
        /// 代理 服务器密码
        /// </summary>
        public string ProxyPwd { get; set; }
        /// <summary>
        /// 代理 服务IP,如果要使用IE代理就设置为ieproxy
        /// </summary>
        public string ProxyIp { get; set; }

        public HttpResultData Get(string Url, string Referer = null, string encode = "UTF-8")
        {
            HttpResultData hrd;
            hrd = Send(Url, Referer, "GET", null, encode);
            return hrd;

        }
        public HttpResultData Post(string Url, string Referer = null, string PostData = "", string encode = "UTF-8")
        {
            byte[] byteArray = Encoding.GetEncoding(encode).GetBytes(PostData);
            HttpResultData hrd;
            hrd = Send(Url, Referer, "POST", byteArray, encode);
            return hrd;
        }

        public HttpResultData Post(string Url, string Referer, byte[] PostData, string encode = "UTF-8")
        {
            HttpResultData hrd;
            hrd = Send(Url, Referer, "POST", PostData, encode);
            return hrd;
        }




        /// <summary>
        /// 请求网页,如果在构造函数时已指定参数，则可以无需任何参数。
        /// </summary>
        /// <returns></returns>
        private HttpResultData Send(string Url = null, string Referer = null, string Method = "GET", byte[] Postdata = null, string encode = "UTF-8")
        {
            HttpResultData hrd = new HttpResultData();
            try
            {

                if (Url != null)
                {
                    this.Url = Url;
                }

                if (Referer != null)
                {
                    this.Referer = Referer;
                }

                if (encode != "UTF-8")
                {
                    _encode = Encoding.GetEncoding(encode);
                }
                hrd.encoding = _encode;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
                                       | SecurityProtocolType.Tls
                                       | (SecurityProtocolType)0x300 //Tls11
                                       | (SecurityProtocolType)0xC00; //Tls12
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(new Uri(this.Url));
                webRequest.Method = Method;
                webRequest.Referer = this.Referer;
                webRequest.UserAgent = _userAgent;
                webRequest.ContentType = ContentType;
                //设置代理
                if (string.IsNullOrWhiteSpace(this.ProxyIp) == false)
                {
                    string[] plist = this.ProxyIp.Split(':');
                    WebProxy myProxy = new WebProxy(plist[0].Trim(), Convert.ToInt32(plist[1].Trim()));
                    //建议连接
                    myProxy.Credentials = new NetworkCredential(this.ProxyUserName, this.ProxyPwd);
                    //给当前请求对象
                    webRequest.Proxy = myProxy;
                }


                if (!string.IsNullOrEmpty(this.Cookie))
                {
                    webRequest.Headers[HttpRequestHeader.Cookie] = this.Cookie;
                }
                else
                {
                    if (Cookies.Count > 0 && AutoCookie)
                    {
                        webRequest.CookieContainer = Cookies;
                    }
                }



                if (Method == "POST" && Postdata != null)
                {
                    webRequest.ContentLength = Postdata.Length;
                    if (ContentType == "text/html")
                    {
                        webRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    }

                    Stream newStream = webRequest.GetRequestStream();
                    newStream.Write(Postdata, 0, Postdata.Length);    //写入参数
                    newStream.Close();
                }

                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                hrd = GetResponse(response, hrd);

                if (response.Cookies.Count > 0 && AutoCookie)
                {
                    Cookies.Add(response.Cookies);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return hrd;
        }
        /// <summary>
        /// 处理返回数据
        /// </summary>
        /// <param name="Res"></param>
        /// <param name="Hrd"></param>
        /// <returns></returns>
        private HttpResultData GetResponse(HttpWebResponse Res, HttpResultData Hrd)
        {
            using (MemoryStream _stream = new MemoryStream())
            {
                //GZIIP处理
                if (Res.ContentEncoding != null && Res.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                {
                    //开始读取流并设置编码方式
                    new GZipStream(Res.GetResponseStream(), CompressionMode.Decompress).CopyTo(_stream, 10240);
                }
                else
                {
                    //开始读取流并设置编码方式
                    Res.GetResponseStream().CopyTo(_stream, 10240);
                }
                //获取Byte
                Hrd.ResponseByte = _stream.ToArray();
            }
            return Hrd;
        }

    }
    public class HttpResultData
    {
        private Encoding _encoder = Encoding.Default;
        /// <summary>
        /// 返回数据编码
        /// </summary>
        public Encoding encoding { get => _encoder; set => _encoder = value; }


        /// <summary>
        /// Http协议返回数据包
        /// </summary>
        public Byte[] ResponseByte = null;
        private string _html = null;
        /// <summary>
        /// Http协议返回的Html代码
        /// </summary>
        public string html
        {
            get
            {
                if (_html == null)
                {
                    if (ResponseByte == null)
                    {
                        _html = "";
                    }
                    else
                    {
                        //转换数据包
                        if (ResponseByte.Length < 5)
                        {
                            _html = "";
                        }
                        else
                        {
                            _html = encoding.GetString(ResponseByte);
                        }
                    }
                }

                return _html;
            }
        }


    }
}
