using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AtolDriver;
using AtolDriver.Models;
using AtolDriver.Utils;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using ESCPOS_NET.Utils;
using FreeKassa.BarcodeScanner;
using FreeKassa.Enum;
using FreeKassa.Model;
using FreeKassa.Model.FiscalDocumentsModel;
using FreeKassa.Model.PrinitngDocumensModel;
using FreeKassa.Payment.Pinpad.Inpas;
using FreeKassa.Payment.Pinpad.Sberbank;
using FreeKassa.Printer.FormForPrinting;
using FreeKassa.Repository;
using FreeKassa.Utils;
using Newtonsoft.Json.Linq;

namespace FreeKassa
{


    public class Test
    {

        
        
       

        public static void Main(string[] args)
        {
            
            var k = new KassaManager();
            k.StartKassa();
            
            
            
            
            // var repository = new MarkedCodeRepository();
            //
            // var a = repository.MarkedWorker<bool>(Work.Save, "0104603934000793215laHIYNedHtxQ93+Ut3");
            // var b = repository.MarkedWorker<bool>(Work.Save, codeList: new List<string>()
            // {
            //     "0104603934000793215laHIYNfdHtxQ93+Ut3",
            //     "0104603934000793215laHIYNfdHtxQ93+Ut351235",
            //     "0104603934000793215laHIYNfdHtxQ93+Ut32346234"
            // });
            //
            //
            // var list = repository.MarkedWorker<List<string>>(Work.Read);
            // repository.MarkedWorker<bool>(Work.Delete, "12312312312");
            //
            //
            //

        }
    }
    
    
    //
    //
    //         k.Successfully += delegate(ChequeFormModel cheque)
    //         {
    //             var d = cheque;
    //             Console.Write("+");
    //
    //             var form = new UserCheque(new EPSON(), cheque);
    //             k.PrintUsersDocument(form.Data);
    //
    //         };
    //         
    //         k.Error += delegate(ChequeFormModel cheque)
    //         {
    //             Console.Write("-");
    //         };
    //
    //         k.RegisterReceipt(new ReceiptModel()
    //             {
    //                 isElectron = true,
    //                 TaxationType = TaxationTypeEnum.TtPatent,
    //                 TypeReceipt = TypeReceipt.Sell
    //             },
    //             new List<BasketModel>()
    //             {
    //                 new BasketModel()
    //                 {
    //                     Cost = 30,
    //                     MeasurementUnit = MeasurementUnitEnum.Piece,
    //                     Name = "Святой источник",
    //                     PaymentObject = PaymentObjectEnum.Commodity,
    //                     Quantity = 1,
    //                     TaxType = TaxTypeEnum.Vat20,
    //                     Ims ="0104603934000779215cx0SpahuIXuI93Wmux"
    //                 },
    //                 new BasketModel()
    //                 {
    //                     Cost = 25,
    //                     MeasurementUnit = MeasurementUnitEnum.Piece,
    //                     Name = "Конфета радума",
    //                     PaymentObject = PaymentObjectEnum.Commodity,
    //                     Quantity = 1,
    //                     TaxType = TaxTypeEnum.Vat20,
    //                 }
    //             },
    //             new PayModel()
    //             {
    //                 PaymentType = PaymentTypeEnum.Electronically,
    //                 Sum = 55
    //             }
    //         );
    //
    //         Console.ReadLine();
    //         
    //     }
    // }
    //
    // public class UserCheque : FormBase
    // {
    //     private void SetMarkingSymbol(BasketModel model)
    //     {
    //         switch (model.Ims)
    //         {
    //             case null:
    //                 return;
    //             
    //             case "":
    //                 return;
    //             
    //             default:
    //                 model.Name += " [M]";
    //                 break;
    //         }
    //     }
    //
    //     public UserCheque(EPSON e, object obj)
    //     {
    //         Data = GetFormData(e, obj);
    //     }
    //     
    //     public sealed override byte[] GetFormData(EPSON vkp80ii, object obj)
    //     {
    //
    //         var chequeFormModel = obj as ChequeFormModel;
    //         
    //         var data = ByteSplicer.Combine(
    //             vkp80ii.CenterAlign(),
    //             vkp80ii.SetStyles(PrintStyle.FontB),
    //             vkp80ii.SetLineSpacingInDots(1),
    //             vkp80ii.PrintLine("ИП ПОВАРНИЦЫНА"),
    //             vkp80ii.PrintLine("НАТАЛЬЯ ВИТАЛЬЕВНА"),
    //             vkp80ii.PrintLine("Кассовый чек"),
    //             vkp80ii.LeftAlign()
    //         );
    //         foreach (var product in chequeFormModel.Products)
    //         {
    //             SetMarkingSymbol(product);
    //             data = CreateProductInCheque(vkp80ii, product, data);
    //         }
    //         
    //         return ByteSplicer.Combine(data,
    //             vkp80ii.SetLineSpacingInDots(1), 
    //             vkp80ii.PrintLine(IdentHelper.SolidLine(IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords("ИТОГО", $"={chequeFormModel.TotalPay}",
    //                 IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(IdentHelper.SolidLine(IdentHelper.Style.FontB)),
    //             vkp80ii.SetStyles(PrintStyle.FontB),
    //             //Налоги должны считаться отдельно
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords(chequeFormModel.TaxesType,
    //                 $"={chequeFormModel.AmountOfTaxes}", IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords(chequeFormModel.TypePay, $"={chequeFormModel.TotalPay}",
    //                 IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords("Кассир", chequeFormModel.CashierName,
    //                 IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(chequeFormModel.CompanyName),
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords("Место расчетов", chequeFormModel.Address,
    //                 IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(chequeFormModel.DateTime),
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords("СНО", chequeFormModel.TaxesType, IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords("Сайт ФНС", "https://nalog.gov.ru",
    //                 IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords("РН ККТ", $"{chequeFormModel.RegisterNumberKKT}",
    //                 IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords("ЗН ККТ", chequeFormModel.SerialNumberKKT, IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords("ИНН", $"{chequeFormModel.Inn}", IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords("ФН", $"{chequeFormModel.FiscalStorageRegisterNumber}",
    //                 IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords("ФД", $"{chequeFormModel.FiscalDocumentNumber}",
    //                 IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords("ФП", $"{chequeFormModel.FiscalFeatureDocument}",
    //                 IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(""),
    //             vkp80ii.LeftAlign(),
    //             vkp80ii.PrintImage(chequeFormModel.QrCode, false, true),
    //             vkp80ii.PrintLine(""),
    //             vkp80ii.PrintLine("")
    //         );
    //
    //
    //     }
    //     
    //     private static byte[] CreateProductInCheque(EPSON vkp80ii ,BasketModel product, byte[] data)
    //     {
    //         return ByteSplicer.Combine(data,
    //             vkp80ii.SetLineSpacingInDots(1),
    //             vkp80ii.PrintLine(product.Name),
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords($"{product.Cost}  *  {product.Quantity}",
    //                 $"={product.Cost * product.Quantity}", IdentHelper.Style.FontB)),
    //             vkp80ii.PrintLine(IdentHelper.ArrangeWords(product.TaxTypeString,
    //                 $"={product.QuantityVat.ToString(CultureInfo.InvariantCulture)}", IdentHelper.Style.FontB)));}
    // }
}

//var lis = GetTicket();

            // k.StartKassa();



            // k.PrintUsersDocument(new List<TicketModel>()
            // {
            //     new TicketModel()
            //     {
            //         Address = "Улица свободы 20",
            //         DateTime = "22.10.2022 14:00",
            //         Places = "Ряд 12 место 10",
            //         Name = "Золотой ключик"
            //     },
            //     new TicketModel()
            //     {
            //         Address = "Улица свободы 20",
            //         DateTime = "22.10.2022 14:00",
            //         Places = "Ряд 12 место 11",
            //         Name = "Золотой ключик"
            //     },
            //     new TicketModel()
            //     {
            //         Address = "Улица свободы 20",
            //         DateTime = "22.10.2022 14:00",
            //         Places = "Ряд 12 место 12",
            //         Name = "Золотой ключик"
            //     }
            // });


            // k.Successfully += delegate(ChequeFormModel cheque)
            // {
            //     var d = cheque;
            //     Console.Write("+");
            // };
            //
            // k.RegisterReceipt(new ReceiptModel()
            //     {
            //         isElectron = true,
            //         TaxationType = TaxationTypeEnum.TtPatent,
            //         TypeReceipt = TypeReceipt.Sell
            //     },
            //     new List<BasketModel>()
            //     {
            //         new BasketModel()
            //         {
            //             Cost = 2900,
            //             MeasurementUnit = MeasurementUnitEnum.Piece,
            //             Name = "Футболка «LOVE»",
            //             PaymentObject = PaymentObjectEnum.Commodity,
            //             Quantity = 1,
            //             TaxType = TaxTypeEnum.Vat20
            //         }
            //     },
            //     new PayModel()
            //     {
            //         PaymentType = PaymentTypeEnum.Electronically,
            //         Sum = 2900
            //     }
            //     // new ClientInfo()
            //     // {
            //     //     EmailOrPhone = "+79991891088"
            //     // }
            // );

            //     Console.ReadLine();
            //     
            //     k.RegisterReceipt( new ReceiptModel()
            //         {
            //             isElectron = false,
            //             TaxationType = TaxationTypeEnum.Osn,
            //             TypeReceipt = TypeReceipt.Sell
            //         }, 
            //         new List<BasketModel>() 
            //         { 
            //             new BasketModel() 
            //             {
            //                 Cost = 10,
            //                 MeasurementUnit = MeasurementUnitEnum.Piece,
            //                 Name = "Фотографии",
            //                 PaymentObject = PaymentObjectEnum.Service,
            //                 Quantity = 5,
            //                 TaxType = TaxTypeEnum.Vat10
            //             }
            //         },
            //         new PayModel()
            //         {
            //             PaymentType = PaymentTypeEnum.Cash,
            //             Sum = 50
            //         }
            //         // new ClientInfo()
            //         // {
            //         //     EmailOrPhone = "+79991891088"
            //         // }
            //     );
            //     
            //     Console.ReadLine();
            //
            // k.RegisterReceipt( new ReceiptModel()
            //     {
            //         isElectron = false,
            //         TaxationType = TaxationTypeEnum.Osn,
            //         TypeReceipt = TypeReceipt.Sell
            //     }, 
            //     new List<BasketModel>() 
            //     { 
            //         new BasketModel() 
            //         {
            //             Cost = 10,
            //             MeasurementUnit = MeasurementUnitEnum.Piece,
            //             Name = "Фотографии",
            //             PaymentObject = PaymentObjectEnum.Service,
            //             Quantity = 5,
            //             TaxType = TaxTypeEnum.Vat10
            //         }
            //     },
            //     new PayModel()
            //     {
            //         PaymentType = PaymentTypeEnum.cash,
            //         Sum = 50
            //     },
            //     new ClientInfo()
            //     {
            //         EmailOrPhone = "+79991891088"
            //     }
            // );
            //     
            //     Console.ReadLine();
            // }
            //
            // private static List<TicketModel> GetTicket()
            // {
            //     return new List<TicketModel>()
            //     {
            //         new TicketModel()
            //         {
            //             // Logo = logo,
            //             Address = "Улица свободы 20",
            //             DateTime = "22.10.2022 14:00",
            //             Places = "Ряд 12 место 10",
            //             Name = "Золотой ключик"
            //         },
            //         new TicketModel()
            //         {
            //             // Logo = logo,
            //             Address = "Улица свободы 20",
            //             DateTime = "22.10.2022 14:00",
            //             Places = "Ряд 12 место 11",
            //             Name = "Золотой ключик"
            //         },
            //         new TicketModel()
            //         {
            //             // Logo = logo,
            //             Address = "Улица свободы 20",
            //             DateTime = "22.10.2022 14:00",
            //             Places = "Ряд 12 место 12",
            //             Name = "Золотой ключик"
            //         }
            //     };
            // }

//         }
//
//
//     }
// }