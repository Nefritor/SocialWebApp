using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialAPI.Models
{
    public class Group
    {
        public string name;
        public string description;
        public string members_count;
        public string photo_big;
        public string latitude;
        public string longitude;
        public string subjectName;

        public Group(string name, string description, string members_count, string photo_big, string latitude, string longitude, string subjectName)
        {
            this.name = name;
            this.description = description;
            this.members_count = members_count;
            this.photo_big = photo_big;
            this.latitude = latitude;
            this.longitude = longitude;
            this.subjectName = subjectName;
        }
    }
}