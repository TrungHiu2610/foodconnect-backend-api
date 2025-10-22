using FoodConnect.Backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FoodConnect.Backend.Domain.Entities
{
    public class ShopRegistrationAsset : BaseEntity
    {
        public string AssetUrl { get; set; }
        public ShopRegistrationAssetTypeEnum AssetType { get; set; }

        public Guid SellerRegistrationId { get; set; }
        public virtual ShopRegistration ShopRegistration { get; set; }
    }
}
