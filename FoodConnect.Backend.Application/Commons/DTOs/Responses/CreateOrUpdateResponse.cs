using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Commons.DTOs.Responses
{
    public class CreateOrUpdateResponse
    {
        public Guid Id { get; set; }
        public bool IsSuccess { get; set; }
    }
}
