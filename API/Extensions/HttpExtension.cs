using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using API.Helpers;

namespace API.Extensions
{
    public static class HttpExtension
    {
        public static void AddPaginationHeader(this HttpResponse response,
            int currentPage,int itemsPerPage,int totalItems,int totalPages)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));

            var paginationHeader=new PaginationHeader( currentPage,  itemsPerPage,  totalItems,  totalPages);
            var options=new JsonSerializerOptions{
                PropertyNamingPolicy=JsonNamingPolicy.CamelCase
            };
            response.Headers.Add("Pagination",JsonSerializer.Serialize(paginationHeader));
            response.Headers.Add("Access-Control-Expose-Headers","Pagination");
        }
    }
}