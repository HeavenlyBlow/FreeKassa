using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using AtolDriver;
using AtolDriver.Models;
using AtolDriver.Utils;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utils;
using FreeKassa.Model;
using FreeKassa.Model.FiscalDocumentsModel;
using FreeKassa.Model.PrinitngDocumensModel;
using FreeKassa.Payment.Pinpad.Inpas;
using FreeKassa.Payment.Pinpad.Sberbank;
using FreeKassa.Printer.FormForPrinting;
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
            
            
            k.Successfully += delegate(ChequeFormModel cheque)
            {
                var d = cheque;
                Console.Write("+");
            };
            
            k.RegisterReceipt(new ReceiptModel()
                {
                    isElectron = true,
                    TaxationType = TaxationTypeEnum.TtPatent,
                    TypeReceipt = TypeReceipt.Sell
                },
                new List<BasketModel>()
                {
                    new BasketModel()
                    {
                        Cost = 2900,
                        MeasurementUnit = MeasurementUnitEnum.Piece,
                        Name = "Футболка «LOVE»",
                        PaymentObject = PaymentObjectEnum.Commodity,
                        Quantity = 1,
                        TaxType = TaxTypeEnum.Vat20
                    }
                },
                new PayModel()
                {
                    PaymentType = PaymentTypeEnum.Electronically,
                    Sum = 2900
                }
            );

            Console.ReadLine();



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

        }


    }
}