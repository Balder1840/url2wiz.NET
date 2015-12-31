using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace url2wiz.NET
{
    class Program
    {
        private static int Wait = 10;//-S
        private static string RootDirectory = AppDomain.CurrentDomain.BaseDirectory;//-D
        private static bool IgnoreRepeat = false;//-I
        private static Dictionary<string, string> Urls = new Dictionary<string, string>();
        private static readonly string URLStart = "URL=";
        private static bool LogErrorUrl = false;//-L
        private static string Url = "";//-U
        private static string UrlFile = "";//-F

        static void Main(string[] args)
        {
            GetUrlShortcutPath(@"D:\Links");

            Console.Write("press ctrl+c to exit...");
            string quit = Console.ReadLine();
            while (true)
            { }
        }

        static void GetUrlShortcutPath(string directory)
        {
            string[] files = Directory.GetFiles(directory,"*.URL");



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
                            if (IgnoreRepeat && Urls.Values.Contains(lineContent))
                            {
                                break;
                            }
                            

                            Urls.Add(,lineContent);
                            break;
                        }
                    }                   
                }
            }

            foreach (var dir in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
            {
                GetUrlShortcutPath(dir);
            }
        }
    }
}
