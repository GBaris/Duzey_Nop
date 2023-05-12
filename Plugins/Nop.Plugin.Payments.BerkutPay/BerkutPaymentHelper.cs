namespace Nop.Plugin.Payments.BerkutPay
{
    /// <summary>
    /// Represents PayPal helper
    /// </summary>
    public class BerkutPaymentHelper
    {

        #region Methods

        public static string GetMDStatusErrorMessage(string mdStatus)
        {
            var result = "Doğrulama Hatası";

            switch (mdStatus)
            {
                case "0":
                    result = "Kart doğrulama başarısız. Lütfen tekrar deneyiniz.";
                    break;
                case "1":
                    result = "Kart doğrulama başarılı.";
                    break;
                case "2":
                    result = "Lütfen bankanız ile iletişime geçip tekrar deneyiniz.";
                    break;
                case "3":
                    result = "Lütfen bankanız ile iletişime geçip tekrar deneyiniz.";
                    break;
                case "4":
                    result = "Kartınız 3D işlemlere kapalı olduğu için doğrulama yapılamıyor.";
                    break;
                case "5":
                    result = "Kartınız 3D işlemlere kapalı olduğu için doğrulama yapılamıyor.";
                    break;
                case "6":
                    result = result + "E06";
                    break;
                case "7":
                    result = result + "E07";
                    break;
                case "8":
                    result = result + "E08";
                    break;
                case "9":
                    result = result + "E09";
                    break;
                default:
                    break;
            }

            return result;
        }

        #endregion

    }
}