using FoodConnect.Backend.Domain.Enums;
using FoodConnect.Backend.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Entities
{
    public class OrderComplaintAsset : BaseEntity, IHardDelete
    {
        public string AssetUrl { get; set; } = string.Empty;
        public OrderComplaintAssetTypeEnum AssetType { get; set; }
        public Guid OrderComplaintId { get; set; }
        public virtual OrderComplaint OrderComplaint { get; set; } = null!;
    }
}
