using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodConnect.Backend.Domain.Enums
{
    public enum SystemConfigTypeEnum
    {
        TermsOfService = 0,
        PrivacyPolicy = 1,
        ReturnPolicy = 2,
        CommissionRate = 3,
        MinWithdrawalAmount = 4,
        Banner = 5,
        Slogan = 6,
        Other = 99
    }
}
