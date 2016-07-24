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
        string access_token;
        int user_id;

        [HttpGet]
        public ActionResult Index(string code)
        {
            /* Веб публикация
             * int client_id = 5556732;
            string client_secret = "Z83Mua1dRjQVkIIa7KuT";
            */
            /*int client_id = 3608614;
            string client_secret = "joAHyo0P6VicaXOrUXx4";
            string urlA = "http://localhost:14314/Authorization";

            // Формирование ссылки
            string redirect_uri = String.Format("https://oauth.vk.com/authorize?client_id={0}&redirect_uri={1}&display=page&v=5.52", client_id, urlA);
            string url = String.Format("https://oauth.vk.com/access_token?client_id={0}&client_secret={1}&redirect_uri={2}&code={3}", client_id, client_secret, redirect_uri, code);
            // Словарь с accessToken'ом и userId
            string xmlCode = SendGet(url);
            Dictionary<string, string> codeDict = CodeParser(xmlCode);
            // Получение xml-файла с информацией о пользователе
            string xmlUser = SendGet("https://api.vk.com/method/users.get.xml?user_ids=" + codeDict["userId"]);

            Users user = new Users(int.Parse(codeDict["userId"]), codeDict["accessToken"], NameParser(xmlUser), "");
            Session["user"] = user;*/
            //return RedirectPermanent("~/Home/Index");
            return View();
        }

        [HttpGet]
        public void Authorization(string token, int u_id)
        {
            string url = String.Format("https://api.vk.com/method/users.get.xml?user_ids={0}&fields=photo_50", u_id);
            string xmlUser = AuthorizationController.SendGet(url);
            access_token = token;
            user_id = u_id;
            Users user = UserParse(xmlUser);
            Session["user"] = user;
            RedirectToAction("Home", "Index");
        }

        public ActionResult LogOut()
        {
            Session["user"] = null;
            return RedirectPermanent("~/Home/Index");
        }

        public Users UserParse(string arg)
        {
            XmlDocument xmlD = new XmlDocument();
            xmlD.LoadXml(arg);

            string fname = xmlD.DocumentElement.ChildNodes[0].ChildNodes[1].InnerText;
            string lname = xmlD.DocumentElement.ChildNodes[0].ChildNodes[2].InnerText;
            string img = xmlD.DocumentElement.ChildNodes[0].ChildNodes[3].InnerText;
            return new Users(user_id, access_token, fname + " " + lname, img);
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