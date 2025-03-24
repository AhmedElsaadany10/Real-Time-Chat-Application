using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class PaginationParams
    {
        private const int MaxPageSize=50;
        private int _pageNum = 1; // Backing field for PageNum
        public int PageNum
        {
            get => _pageNum;
            set => _pageNum = (value < 1) ? 1 : value; // Ensure at least 1
        }
        private int _pageSize=10;

        public int PageSize{
            get=>_pageSize;
            set=>_pageSize=(value > MaxPageSize)? MaxPageSize : value;
        }
    }
}