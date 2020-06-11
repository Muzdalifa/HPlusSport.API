using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HPlusSport.API.Classes
{
    public class QueryParameters // this class hold all comman query parameters
    {
        const int _maxSize = 100; //our maximum size
        private int _size = 50; //our default size, depends on how many product you have. here we dont have many  product

        public int Page { get; set; } //which page you want to see, we can skip the other items so that you can see only you want, this also depends on how you divide your pge
        public int Size //how many items do we want to see, we can write as  .Take(queryParameters.Size)
        {
            get
            {
                return _size;
            }
            set
            {
                _size = Math.Min(_maxSize, value); //what if we get -ve value, its better to do validation
            }
        }

        public string SortBy { get; set; } = "Id";
        private string _sortOrder = "asc";
        public string SortOrder
        {
            get
            {
                return _sortOrder;
            }
            set
            {
                if(value == "asc" || value == "desc")
                {
                    _sortOrder = value;
                }
            }
        }

    }
}
