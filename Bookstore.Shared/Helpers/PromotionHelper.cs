using Bookstore.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Helpers
{
    public class PromotionHelper
    {
        public static Promotion? GetBestPromotionFromList(IEnumerable<Promotion> promotions, decimal originalPrice)
        {
            if (promotions == null || !promotions.Any())
                return null;

            return promotions.OrderByDescending(p =>
                p.DiscountType == 1
                    ? (originalPrice * p.DiscountValue / 100)
                    : p.DiscountValue 
            ).FirstOrDefault();
        }
    }
}
