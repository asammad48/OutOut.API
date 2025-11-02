using OutOut.Constants.Enums;

namespace OutOut.Core.Utils
{
    public static class EnumUtils
    {
        public static PaymentStatus FromTelrStatus(this string input)
        {
            var result = PaymentStatus.Unknown;
            switch (input)
            {
                case "Pending":
                    result = PaymentStatus.Pending;
                    break;
                case "Paid":
                    result = PaymentStatus.Paid;
                    break;
                case "Cancelled":
                    result = PaymentStatus.Cancelled;
                    break;
                case "Declined":
                    result = PaymentStatus.Declined;
                    break;
                case "Expired":
                    result = PaymentStatus.Expired;
                    break;
                case "Authorised":
                    result = PaymentStatus.OnHold;
                    break;
            }
            return result;
        }
    }
}
