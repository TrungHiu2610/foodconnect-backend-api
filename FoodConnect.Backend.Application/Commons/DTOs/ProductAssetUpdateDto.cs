using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Application.Commons.DTOs
{
    public class ProductAssetUpdateDto
    {
        public Guid Id { get; set; }
        public string? AssetDescription { get; set; }
        public bool? IsThumbnail { get; set; }
    }
}
