using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Commons.Models
{
    public class SortInfo
    {
        public string PropertyName { get; set; }
        public bool IsAscending { get; set; }
        public SortInfo(string propertyName, bool isAscending)
        {
            PropertyName = propertyName;
            IsAscending = isAscending;
        }
    }
}
