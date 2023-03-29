using System.Globalization;
using ESCPOS_NET.ConsoleTest;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using FreeKassa.Model;
using FreeKassa.Model.FiscalDocumentsModel;

namespace FreeKassa.Printer.FormForPrinting.FiscalDocuments
{
    public static class ChequeForm
    {
        //TODO нужно разобраться с налогами и как их выводить
        public static byte[] GetChequeForm(EPSON vkp80ii ,ChequeFormModel chequeFormModel)
        {
            // var vkp80ii = new EPSON();
            // ChequeFormModel chequeFormModel
            // var chequeFormModel = (ChequeFormModel)model;
            var data = ByteSplicer.Combine(
                vkp80ii.CenterAlign(),
                vkp80ii.SetStyles(PrintStyle.FontB),
                vkp80ii.PrintLine("Кассовый чек"),
                vkp80ii.LeftAlign(),
                vkp80ii.SetLineSpacingInDots(3)
            );
            foreach (var product in chequeFormModel.Products)
            {
                data = CreateProductInCheque(vkp80ii, product, data);
            }
            
            return ByteSplicer.Combine(data, 
                vkp80ii.PrintLine(IdentHelper.SolidLine(IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(IdentHelper.ArrangeWords("ИТОГО", $"={chequeFormModel.TotalPay}",
                    IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(IdentHelper.SolidLine(IdentHelper.Style.FontB)),
                vkp80ii.SetStyles(PrintStyle.FontB),
                //Налоги должны считаться отдельно
                vkp80ii.PrintLine(IdentHelper.ArrangeWords(chequeFormModel.TaxesType,
                    $"={chequeFormModel.AmountOfTaxes}", IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(IdentHelper.ArrangeWords(chequeFormModel.TypePay, $"={chequeFormModel.TotalPay}",
                    IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(IdentHelper.ArrangeWords("Кассир", chequeFormModel.CashierName,
                    IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(chequeFormModel.CompanyName),
                vkp80ii.PrintLine(IdentHelper.ArrangeWords("Место расчетов", chequeFormModel.Address,
                    IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(chequeFormModel.DateTime),
                vkp80ii.PrintLine(IdentHelper.ArrangeWords("СНО", chequeFormModel.TaxesType, IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(IdentHelper.ArrangeWords("Сайт ФНС", "https://nalog.gov.ru",
                    IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(IdentHelper.ArrangeWords("РН ККТ", $"{chequeFormModel.RegisterNumberKKT}",
                    IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(IdentHelper.ArrangeWords("ЗН ККТ", chequeFormModel.SerialNumberKKT, IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(IdentHelper.ArrangeWords("ИНН", $"{chequeFormModel.Inn}", IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(IdentHelper.ArrangeWords("ФН", $"{chequeFormModel.FiscalStorageRegisterNumber}",
                    IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(IdentHelper.ArrangeWords("ФД", $"{chequeFormModel.FiscalDocumentNumber}",
                    IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(IdentHelper.ArrangeWords("ФП", $"{chequeFormModel.FiscalFeatureDocument}",
                    IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(""),
                vkp80ii.LeftAlign(),
                vkp80ii.PrintImage(chequeFormModel.QrCode, false, true),
                vkp80ii.PrintLine(""),
                vkp80ii.PrintLine("")
            );


        }

        private static byte[] CreateProductInCheque(EPSON vkp80ii ,BasketModel product, byte[] data)
        {
            return ByteSplicer.Combine(data,
                vkp80ii.PrintLine(product.Name),
                vkp80ii.PrintLine(IdentHelper.ArrangeWords($"{product.Cost}  *  {product.Quantity}",
                    $"={product.Cost * product.Quantity}", IdentHelper.Style.FontB)),
                vkp80ii.PrintLine(IdentHelper.ArrangeWords(product.TaxTypeString,
                    $"={product.QuantityVat.ToString(CultureInfo.InvariantCulture)}", IdentHelper.Style.FontB)));}

        
    }
}