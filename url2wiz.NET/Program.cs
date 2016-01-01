using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Specialized;
using static System.Uri;
using System.Net;
using System.Threading;

namespace url2wiz.NET
{
    class Program
    {
        #region variables
        private static int Interval = 20 * 1000;//-S
        private static string RootDirectory = AppDomain.CurrentDomain.BaseDirectory;//-D
        private static bool IgnoreRepeat = true;//-I
        private static NameValueCollection Urls = new NameValueCollection();
        private static readonly string URLStart = "URL=";
        private static bool LogFailedUrl = true;//-L
        private static string Url = "";//-W
        private static string UrlFile = "";//-F
        private static string User = string.Empty;
        private static readonly string RequestDataFormat = "url={0}&folder={1}&user={2}&content-only=true";
        private static readonly string RequestUrlFormat = @"http://note.wiz.cn/api/gather/add?type=url2wiz&data={0}&{1}";
        private static readonly string ResponseSuccessCode = "\"return_code\":200, \"return_message\":\"success\"";
        #region usage
        private static readonly string Usage = @"Usage:
-D:	the root directory of your internate shortcuts, 
	default as the same directory of the tool
-U:	your wiz email ID, mandatory
-S:	interval of each request to wiz API, default as 20s, unit as seconds
-L:	log those failed to request, save to the same directory as -D specified
	default as true
-I:	if ignore the repeat urls, default as true
-W:	single web url to add
-F:	a file contains the url to add, format as [path(\t)url].
	this will be usefull when adding the failed ones
-H/-?	Help

example:
	url2wiz.NET.exe -D C:\Links -U YourUserId -S 10 -L -I false
			-W http://www.wiz.cn -F C:\yourfaillist.txt";
        #endregion
        #endregion

        static void Main(string[] args)
        {
            //the service api has 2 parameters,data and timestamp
            //http://note.wiz.cn/url2wiz?url=http%3A%2F%2Fwww.wiz.cn%2F&folder=%2FMy%20Notes%2F&user=YourUserId&content-only=true
            //http://note.wiz.cn/api/gather/add?type=url2wiz&data=url%3Dhttp%3A%2F%2Fwww.wiz.cn%2F%26folder%3D%252FMy%2520Notes%252F%26user%3DYourUserId%26content-only%3Dtrue&1451585829953
            //and data has url(to add in wiz),folder(location in wiz),user(wiz email ID) and content-only(true/false)
            //url=http%3A%2F%2Fwww.wiz.cn%2F&folder=%2FMy%20Notes%2F&user=YourUserId&content-only=true
            //response data as JSON, { "custom_id":"7495b875-c6e7-494b-8298-eaea256a877c","id":5718754,"return_code":200,"return_message":"success"}

            if (ParseArguments(args))
            {
                using (Logger logger = new Logger(RootDirectory))
                {
                    ExtractUrlShortcutPath(RootDirectory);
                    if (Urls.GetAllValues().Count() > 0)
                    {
                        SendRequests(Logger logger);
                    }
                }
            }

            Console.Write("press ctrl+c to exit...");
            string quit = Console.ReadLine();
            while (true)
            { }
        }

        private static bool ParseArguments(string[] args)
        {
            CommandLineParser clp = new CommandLineParser(args);
            int _interval;
            if (int.TryParse(clp["S"], out _interval))
            {
                Interval = _interval * 1000;
            }
            bool _logFailUrl;
            if(bool.TryParse(clp["L"], out _logFailUrl))
            {
                LogFailedUrl = _logFailUrl;
            }
            bool _ignoreRepeat;
            if (bool.TryParse(clp["I"], out _ignoreRepeat))
            {
                IgnoreRepeat = _ignoreRepeat;
            }

            User = !string.IsNullOrEmpty(clp["U"]) && Directory.Exists(clp["U"]) ? clp["U"] : User;
            RootDirectory = !string.IsNullOrEmpty(clp["D"]) && Directory.Exists(clp["D"]) ? clp["D"] : RootDirectory;
            Url = !string.IsNullOrEmpty(clp["W"]) && Directory.Exists(clp["W"]) ? clp["W"] : Url;
            UrlFile = !string.IsNullOrEmpty(clp["F"]) && Directory.Exists(clp["F"]) ? clp["F"] : UrlFile;

            return ValidateArguments(clp);
        }

        private static bool ValidateArguments(CommandLineParser clp)
        {
            if(clp.Contains("H")||clp.Contains("?"))
            {
                PrintUsage();
                return false;
            }
            if(string.IsNullOrEmpty(clp["U"]))
            {
                Console.WriteLine("please specify -U");
                Console.WriteLine("for more help, plesae use -H/-?");
                return false;
            }
            return true;
        }

        private static void PrintUsage()
        {
            Console.WriteLine(Usage);
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

        private static void SendRequests(Logger logger)
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
                                        Thread.Sleep(Interval);
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                    Thread.Sleep(Interval);
                    //Log failed request
                    if(LogFailedUrl)
                    {
                        logger.Log(key, urlToAdd);
                    }
                }
            }
        }
    }
}
