using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HPlusSport.API.Classes
{
    public class QueryParameters
    {
        const int _maxSize = 100; //our maximum size
        private int _size = 50; //our default size, depends on how many product you have. here we dont have many  product

        public int Page { get; set; }
        public int Size
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

    }
}
