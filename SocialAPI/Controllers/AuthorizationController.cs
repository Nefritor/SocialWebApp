using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.IO;
using SocialAPI.Models;
using System.Xml;

namespace SocialAPI.Controllers
{
    public class AuthorizationController : Controller
    {
        [HttpGet]
        public RedirectResult Index(string code)
        {
            int client_id = 3608614;
            string client_secret = "joAHyo0P6VicaXOrUXx4";
            // Формирование ссылки
            string redirect_uri = String.Format("https://oauth.vk.com/authorize?client_id={0}&redirect_uri={1}&display=page&v=5.52", client_id, "http://localhost:14314/Authorization");
            string url = String.Format("https://oauth.vk.com/access_token?client_id={0}&client_secret={1}&redirect_uri={2}&code={3}", client_id, client_secret, redirect_uri, code);
            // Словарь с accessToken'ом и userId
            string xmlCode = SendGet(url);
            Dictionary<string, string> codeDict = CodeParser(xmlCode);
            // Получение xml-файла с информацией о пользователе
            string xmlUser = SendGet("https://api.vk.com/method/users.get.xml?user_ids=" + codeDict["userId"]);

            Users user = new Users(int.Parse(codeDict["userId"]), codeDict["accessToken"], NameParser(xmlUser));
            Session["user"] = user;
            return RedirectPermanent("~/Home/Index");
        }

        public static string SendGet(string url)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string result = sr.ReadToEnd();
            sr.Close();
            return result;
        }

        public static string NameParser(string arg)
        {
            XmlDocument xmlD = new XmlDocument();
            xmlD.LoadXml(arg);
            arg = xmlD.DocumentElement.ChildNodes[0].ChildNodes[1].InnerText + " " + xmlD.DocumentElement.ChildNodes[0].ChildNodes[2].InnerText;
            return arg;
        }

        public static Dictionary<string, string> CodeParser(string arg)
        {
            // Извращенная очистка строки
            arg = Clear(arg);
            arg = arg.Remove(0, arg.IndexOf(':') + 1);
            arg = arg.Remove(arg.IndexOf(','), arg.LastIndexOf(",") - (arg.IndexOf(',') + 1));
            arg = arg.Replace("user_id:", "");
            // Заполнение словаря
            string[] splitLine = arg.Split(',');
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("accessToken", splitLine[0].Trim());
            dict.Add("userId", splitLine[1].Trim());
            return dict;
        }

        public static string Clear(string arg)
        {
            arg = arg.Replace("\"", "");
            arg = arg.Replace("'", "");
            arg = arg.Replace("{", "");
            arg = arg.Replace("}", "");
            return arg;
        }
	}
}