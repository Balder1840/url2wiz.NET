using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Specialized;
using static System.Uri;
using System.Net;

namespace url2wiz.NET
{
    class Program
    {
        private static int Wait = 10;//-S
        private static string RootDirectory = AppDomain.CurrentDomain.BaseDirectory;//-D
        private static bool IgnoreRepeat = true;//-I
        private static NameValueCollection Urls = new NameValueCollection();
        private static readonly string URLStart = "URL=";
        private static bool LogErrorUrl = false;//-L
        private static string Url = "";//-W
        private static string UrlFile = "";//-F
        private static string User = "balder1840";
        private static readonly string RequestDataFormat = "url={0}&folder={1}&user={2}&content-only=true";
        private static readonly string RequestUrlFormat = @"http://note.wiz.cn/api/gather/add?type=url2wiz&data={0}&{1}";
        private static readonly string ResponseSuccessCode = "\"return_code\":200, \"return_message\":\"success\"";

        static void Main(string[] args)
        {
            //the service api has 2 parameters,data and timestamp
            //http://note.wiz.cn/api/gather/add?type=url2wiz&data=url%3Dhttp%253A%252F%252Fwww.cnblogs.com%252Fxiaocai20091687%252Fp%252F5092548.html%26folder%3D%252FMy%2520Notes%252F%26user%3Dbalder1840%26content-only%3Dtrue&1451585829953
            //and data has url(to add in wiz),folder(location in wiz),user(wiz email ID) and content-only(true/false)
            //url=http%3A%2F%2Fwww.wiz.cn%2F&folder=%2FMy%20Notes%2F&user=balder&content-only=true
            //response data as JSON, { "custom_id":"7495b875-c6e7-494b-8298-eaea256a877c","id":5718754,"return_code":200,"return_message":"success"}


            ExtractUrlShortcutPath(@"C:\Projects\url2wiz.NET\url2wiz.NET\bin\Debug\Links\");
            if(Urls.GetAllValues().Count() > 0)
            {
                SendRequests();
            }

            Console.Write("press ctrl+c to exit...");
            string quit = Console.ReadLine();
            while (true)
            { }
        }

        static void ExtractUrlShortcutPath(string directory)
        {
            string[] files = Directory.GetFiles(directory,"*.URL");
            //extract the parameter name folder
            string folder = string.Empty;
            //default as My Notes for shortcuts in root directory 
            if (directory == RootDirectory)
            {
                folder = @"/My Notes/";
            }
            else
            {
                folder = directory.Substring(RootDirectory.Length-1).Replace(@"\", "/");
            }

            foreach (string strFile in files)
            {
                using (StreamReader sr = File.OpenText(strFile))
                {
                    while(!sr.EndOfStream)
                    {
                        string lineContent = sr.ReadLine();
                        if (lineContent.StartsWith(URLStart, StringComparison.OrdinalIgnoreCase))
                        {
                            lineContent = lineContent.Substring(URLStart.Length);
                            var values = Urls.GetAllValues();
                            if (IgnoreRepeat && Urls.GetAllValues().Contains(lineContent))
                            {
                                break;
                            }
                            
                            Urls.Add(folder, lineContent);
                            break;
                        }
                    }                   
                }
            }

            foreach (var dir in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
            {
                ExtractUrlShortcutPath(dir);
            }
        }

        private static void SendRequests()
        {
            foreach(var key in Urls.AllKeys)
            {
                foreach(var urlToAdd in Urls.GetValues(key))
                {
                    string requestData = string.Format(RequestDataFormat, EscapeDataString(urlToAdd), EscapeDataString(key), EscapeDataString(User));
                    string requestUrl = string.Format(RequestUrlFormat, EscapeDataString(requestData), DateTime.Now.ToUtcTicksLikeJS());
                    HttpWebRequest request = WebRequest.CreateHttp(requestUrl);
                    request.Method = WebRequestMethods.Http.Get;

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (var stream = response.GetResponseStream())
                            {
                                using (var reader = new StreamReader(stream))
                                {
                                    var responseJson = reader.ReadToEnd();
                                    if (responseJson.Contains(ResponseSuccessCode))
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    //Log error request

                }
            }
        }
    }
}
