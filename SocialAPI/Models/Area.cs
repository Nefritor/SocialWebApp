using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialAPI.Models
{
    public class Area
    {
        public string district_id;
        public string region_id;
        public string name;

        public Area(string district_id, string region_id, string name)
        {
            this.district_id = district_id;
            this.region_id = region_id;
            this.name = name;
        }
    }
}