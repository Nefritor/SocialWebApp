using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SocialAPI.Models;
using System.Xml;
using HtmlAgilityPack;
using System.Data.SqlClient;

namespace SocialAPI.Controllers
{
    public class HomeController : Controller
    {
        static string q, type, q1, areaType, areaId, areaCity;
        static List<Group> groups = new List<Group>();
        static List<string> otherGroups = new List<string>();
        static List<string> cities = new List<string>();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Info()
        {
            bool a = false;
            if (q1 == q) a = true;
            return Json(a, JsonRequestBehavior.AllowGet);
        }

        
        public JsonResult GetArea(string table)
        {
            List<Area> area = new List<Area>();
            string connString = @"Data Source=DNS\QUSIJUE;Initial Catalog=SocialAPI;Integrated Security=True";
            SqlConnection cnn = new SqlConnection(connString);
            SqlCommand cmd = new SqlCommand("SELECT * FROM " + table, cnn);
            cnn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string district_id = null,
                    name = null,
                    region_id = null;

                if (table == "District")
                {
                    district_id = reader.GetValue(0).ToString();
                    name = reader.GetValue(1).ToString();
                    region_id = null;
                }
                else if (table == "Region")
                {
                    district_id = null;
                    name = reader.GetValue(1).ToString();
                    region_id = reader.GetValue(0).ToString();
                }
                area.Add(new Area(district_id, region_id, name));
            }
            cnn.Close();
            return Json(area, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetData()
        {
            if (q == null) return null;
            Users userA = (Users)Session["user"];
            List<int> groupIdListA = new List<int>();
            List<Group> groupsListA = new List<Group>();
            if (areaType != "3")
            {
                
                int offset = 0;
                while (true)
                {
                    string url = String.Format("https://api.vk.com/method/groups.search.xml?q={0}&type={1}&v=5.52&access_token={2}&count=1000&offset={3}&country_id=1", q, type, userA.accessToken, offset);
                    string xmlGroups = AuthorizationController.SendGet(url);
                    offset += 1000;
                    HtmlNodeCollection nodeOfIds = SelectNodes(xmlGroups, "//id");
                    if (nodeOfIds != null)
                    {
                        foreach (HtmlNode g in nodeOfIds)
                            groupIdListA.Add(int.Parse(g.InnerHtml));
                        
                    }
                    else break;
                }
                groupsListA = GroupFill(groupIdListA);
            }
            else if (areaType == "3")
            {
                string urlA = String.Format("https://api.vk.com/method/database.getCities.xml?country_id=1&q={0}&count=1000", areaCity);
                string xmlCityA = AuthorizationController.SendGet(urlA);
                HtmlNodeCollection nodeOfIdsA = SelectNodes(xmlCityA, "//cid");
                if (nodeOfIdsA != null)
                {
                    foreach (HtmlNode g in nodeOfIdsA)
                        cities.Add(g.InnerHtml);
                }
                // Получение xml-файла со списком групп
                foreach (string city in cities)
                {
                    string url = String.Format("https://api.vk.com/method/groups.search.xml?q={0}&type={1}&v=5.52&access_token={2}&city_id={3}&count=500", q, type, userA.accessToken, city);
                    string xmlGroups = AuthorizationController.SendGet(url);
                    HtmlNodeCollection nodeOfIds = SelectNodes(xmlGroups, "//id");
                    // Получение id групп
                    if (nodeOfIds == null) continue;
                    foreach (HtmlNode g in nodeOfIds)
                        groupIdListA.Add(int.Parse(g.InnerHtml));
                }
                // Создание коллекции групп
                groupsListA = GroupFill(groupIdListA);
            }
            q1 = q;
            Session["flag"] = true;
            System.IO.File.WriteAllLines(@"C:\GroupList.txt", otherGroups);
            return Json(groupsListA, JsonRequestBehavior.AllowGet);
        }

        public RedirectResult Search()
        {
            groups.Clear();
            cities.Clear();
            q = Request.Form["q"];
            type = Request.Form["type"];
            areaType = Request.Form["areaType"];
            areaId = Request.Form["area"];
            areaCity = Request.Form["areaCity"];
            return RedirectPermanent("~/Home/Index");
        }

        public HtmlNodeCollection SelectNodes(string xml, string tag)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(xml);
            HtmlNodeCollection node = doc.DocumentNode.SelectNodes(tag);
            return node;
        }

        public List<Group> GroupFill(List<int> groupIdList)
        {
            int num = 400;
            List<Group> groupList = new List<Group>();
            double count = groupIdList.Count / num + 1;
            for (int i = 0; i <= count; i++)
            {
                string ids = "";
                int idCount = 0;
                for (int j = num * i; j < num + (num * i) && j < groupIdList.Count; j++)
                {
                    if (j % num == 0)
                    {
                        ids = ids + groupIdList[j];
                        idCount++;
                    }
                    else
                    {
                        ids = ids + "," + groupIdList[j];
                        idCount++;
                    }
                }
                string url = String.Format("https://api.vk.com/method/groups.getById.xml?group_ids={0}&fields=place,description,members_count,city", ids);
                string xmlGroup = AuthorizationController.SendGet(url);
                GroupParse(xmlGroup, idCount);
            }
            return groups;
        }

        public void GroupParse(string arg, int idCount)
        {
            for (int i = 0; i < idCount; i++)
            {
                XmlDocument xmlD = new XmlDocument();
                xmlD.LoadXml(arg);
                if (xmlD.DocumentElement.ChildNodes[i].ChildNodes[5].Name == "place" && int.Parse(xmlD.DocumentElement.ChildNodes[i].ChildNodes[8].InnerText) != 0)
                {
                    string gid = xmlD.DocumentElement.ChildNodes[i].ChildNodes[0].InnerText;
                    string name = xmlD.DocumentElement.ChildNodes[i].ChildNodes[1].InnerText;
                    string description = xmlD.DocumentElement.ChildNodes[i].ChildNodes[6].InnerText;
                    string latitude = xmlD.DocumentElement.ChildNodes[i].ChildNodes[5].ChildNodes[2].InnerText;
                    string longitude = xmlD.DocumentElement.ChildNodes[i].ChildNodes[5].ChildNodes[3].InnerText;
                    string count = xmlD.DocumentElement.ChildNodes[i].ChildNodes[7].InnerText;
                    string img = xmlD.DocumentElement.ChildNodes[i].ChildNodes[10].InnerText;
                    string city = xmlD.DocumentElement.ChildNodes[i].ChildNodes[8].InnerText;
                    string subjectName = "false";
                    if (areaId != null) { subjectName = areaId; }
                    bool isSubject = true;
                    if (areaType == "1") { isSubject = false; }
                    groups.Add(new Group(gid, name, description, count, img, latitude, longitude, subjectName, isSubject, true));
                }
                else
                {
                    try
                    {
                        string gid = xmlD.DocumentElement.ChildNodes[i].ChildNodes[0].InnerText;
                        string name = xmlD.DocumentElement.ChildNodes[i].ChildNodes[1].InnerText;
                        string description = xmlD.DocumentElement.ChildNodes[i].ChildNodes[5].InnerText;
                        string count = xmlD.DocumentElement.ChildNodes[i].ChildNodes[6].InnerText;
                        string img = xmlD.DocumentElement.ChildNodes[i].ChildNodes[10].InnerText;
                        string city = xmlD.DocumentElement.ChildNodes[i].ChildNodes[7].InnerText;
                        string subjectName = "false";
                        if (areaId != null) { subjectName = areaId; }
                        bool isSubject = true;
                        if (areaType == "1") { isSubject = false; }
                        //groups.Add(new Group(gid, name, description, count, img, "0", "0", subjectName, isSubject, false));
                        string hr = "####################################################";
                        string nl = Environment.NewLine;
                        otherGroups.Add(string.Format("Название: {0}{1}Описание: {2}{3}Количество участников: {4}{5}Ссылка: http://vk.com/club{6}{7}{8}", name, nl, description, nl, count, nl, gid, nl, hr));
                    }
                    catch { }
                }
            }
        }

        public void CityParse(string arg)
        {
            HtmlNodeCollection nodeOfIds = SelectNodes(arg, "//cid");
            foreach (HtmlNode g in nodeOfIds)
                cities.Add(g.InnerHtml);
        }
    }
}