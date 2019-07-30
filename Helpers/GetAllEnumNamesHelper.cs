using SigortaTakipSistemi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SigortaTakipSistemi.Helpers
{
    public static class GetAllEnumNamesHelper
    {
        public static List<Insurances> GetEnumName(List<Insurances> insurances)
        {
            foreach (var insurance in insurances)
            {
                if (insurance.InsurancePaymentType == 0)
                {
                    insurance.InsurancePaymentTypeName = EnumExtensionsHelper.GetDisplayName(Insurances.InsurancePaymentTypeEnum.CASH);
                }
                if (insurance.InsurancePaymentType == 1)
                {
                    insurance.InsurancePaymentTypeName = EnumExtensionsHelper.GetDisplayName(Insurances.InsurancePaymentTypeEnum.CREDIT_CARD);
                }
                if (insurance.InsurancePaymentType == 2)
                {
                    insurance.InsurancePaymentTypeName = EnumExtensionsHelper.GetDisplayName(Insurances.InsurancePaymentTypeEnum.UNPAID);
                }
            }

            foreach (var insurance in insurances)
            {
                if (insurance.InsuranceType == 0)
                {
                    insurance.InsuranceTypeName = EnumExtensionsHelper.GetDisplayName(Insurances.InsuranceTypeEnum.NEW);
                }
                if (insurance.InsuranceType == 1)
                {
                    insurance.InsuranceTypeName = EnumExtensionsHelper.GetDisplayName(Insurances.InsuranceTypeEnum.RENEWAL);
                }
                if (insurance.InsuranceType == 2)
                {
                    insurance.InsuranceTypeName = EnumExtensionsHelper.GetDisplayName(Insurances.InsuranceTypeEnum.USED_CAR);
                }
            }

            return insurances;
        }
    }
}
