using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialAPI.Models
{
    public class Group
    {
        public string gid;
        public string name;
        public string description;
        public string members_count;
        public string photo_big;
        public string latitude;
        public string longitude;
        public string subjectName;
        public bool isSubject;
        public bool isMarked;

        public Group(string gid, string name, string description, string members_count, string photo_big, string latitude, string longitude, string subjectName, bool isSubject, bool isMarked)
        {
            this.gid = gid;
            this.name = name;
            this.description = description;
            this.members_count = members_count;
            this.photo_big = photo_big;
            this.latitude = latitude;
            this.longitude = longitude;
            this.subjectName = subjectName;
            this.isSubject = isSubject;
            this.isMarked = isMarked;
        }
    }
}