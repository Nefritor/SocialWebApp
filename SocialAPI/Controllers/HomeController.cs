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
        static string q, type, q1, numb, areaType;
        static List<Group> groups = new List<Group>();

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
            string connString = @"Data Source=Nefritor-pc\mssqlserver1;Initial Catalog=SocialAPI;Integrated Security=True";
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
            return Json(area, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetData()
        {
            Users user = (Users)Session["user"];
            List<int> groupIdList = new List<int>();
            List<Group> groupsList = new List<Group>();
            // Получение xml-файла со списком групп
            string url = String.Format("https://api.vk.com/method/groups.search.xml?q={0}&type={1}&v=5.52&access_token={2}&count={3}", q, type, user.accessToken, numb);
            string xmlGroups = AuthorizationController.SendGet(url);
            HtmlNodeCollection nodeOfIds = SelectNodes(xmlGroups, "//id");
            // Получение id групп
            if (q == null) { return null; }
            else
            {
                foreach (HtmlNode g in nodeOfIds)
                    groupIdList.Add(int.Parse(g.InnerHtml));
                // Создание коллекции групп
                groupsList = GroupFill(groupIdList);
                q1 = q;
                return Json(groupsList, JsonRequestBehavior.AllowGet);
            }
        }

        public RedirectResult Search()
        {
            groups.Clear();
            numb = Request.Form["numb"];
            q = Request.Form["q"];
            type = Request.Form["type"];
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
                    string name = xmlD.DocumentElement.ChildNodes[i].ChildNodes[1].InnerText;
                    string description = xmlD.DocumentElement.ChildNodes[i].ChildNodes[6].InnerText;
                    string latitude = xmlD.DocumentElement.ChildNodes[i].ChildNodes[5].ChildNodes[2].InnerText;
                    string longitude = xmlD.DocumentElement.ChildNodes[i].ChildNodes[5].ChildNodes[3].InnerText;
                    string count = xmlD.DocumentElement.ChildNodes[i].ChildNodes[7].InnerText;
                    string img = xmlD.DocumentElement.ChildNodes[i].ChildNodes[10].InnerText;
                    string city = xmlD.DocumentElement.ChildNodes[i].ChildNodes[8].InnerText;
                    groups.Add(new Group(name, description, count, img, latitude, longitude));
                }
            }
        }
    }
}