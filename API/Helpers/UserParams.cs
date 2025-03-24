using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class UserParams:PaginationParams
    {

        public string CurrentUser { get; set; }
        public string Gender { get; set; }
        public string OrderBy { get; set; }="LastActive";
    }
}