using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assembly.Projecto.Final.Services.Pagination
{
    public class Pagination<T> where T : class
    {
        public List<T> items { get; private set; }
        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages 
        { 
            get 
            {
                if (PageSize == 0) return 0;
                return (int)Math.Ceiling((double)TotalCount / PageSize); 
            }
        }

        public Pagination(int pageNumber, int pageSize,int totalCount) 
        { 
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
            this.TotalCount = totalCount;
        }

        public static Pagination<T> Create(List<T> items, int pageNumber, int pageSize, int totalCount)
        {
            var pagination = new Pagination<T>(pageNumber, pageSize, totalCount);
            pagination.items = items;
            return pagination;
        }
    }
}
