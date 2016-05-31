using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace SocialAPI.Models
{
    public class Area
    {
        static public Dictionary<string, string> Query(string table)
        {
            Dictionary<string, string> dict = new Dictionary<string,string>();
            string connString = @"Data Source=DNS\Qusijue;Initial Catalog=SocialAPI;Integrated Security=True";
            SqlConnection cnn = new SqlConnection(connString);
            SqlCommand cmd = new SqlCommand("SELECT * FROM " + table, cnn);
            cnn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dict.Add(reader.GetValue(0).ToString(), reader.GetValue(1).ToString());
            }
            return dict;
        }
    }
}