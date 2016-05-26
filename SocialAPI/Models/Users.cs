using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialAPI.Models
{
    public class Users
    {
        public int userId;
        public string accessToken;
        public string name;

        public Users(int userId, string accessToken, string name)
        {
            this.userId = userId;
            this.accessToken = accessToken;
            this.name = name;
        }
    }
}