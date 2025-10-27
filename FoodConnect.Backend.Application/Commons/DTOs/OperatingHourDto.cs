using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Commons.DTOs
{
    public class OperatingHourDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public string OpenTime { get; set; } = string.Empty;
        public string CloseTime { get; set; } = string.Empty;
    }
}
