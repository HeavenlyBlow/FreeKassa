using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using AtolDriver;
using AtolDriver.models;
using ESCPOS_NET.Utils;
using FreeKassa.Model;
using FreeKassa.Model.PrinitngDocumensModel;
using FreeKassa.Utils;
using Newtonsoft.Json.Linq;

namespace FreeKassa
{
    public class Test
    {
        
        

        public static void Main(string[] args)
        {
            // var _kktModel = (KKTModel)ConfigHelper.GetSettings("KKT");
            // var inter = new Interface(_kktModel.Port, _kktModel.PortSpeed);
            // inter.OpenConnection();
            // var date = DateTime.Now;
            // var dateNow = DateTime.Now;
            // var sta = inter.GetShiftStatus();
            // if (date.Day == dateNow.Day && date.Month == dateNow.Month)
            // {
            //     if (dateNow >= _kktModel.OpenShifts && dateNow < _kktModel.CloseShifts)
            //     {
            //         if (sta == 0)
            //         {
            //             inter.SetOperator(_kktModel.CashierName, _kktModel.OperatorInn);
            //             var openShiftsAnswer = inter.OpenShift();
            //             return;
            //         };
            //     }
            //     else
            //     {
            //         if (sta == 1) inter.CloseShift();
            //     }
            //     return;
            // }
            // if (dateNow.Hour >= _kktModel.OpenShifts.Hour && dateNow.Hour < _kktModel.CloseShifts.Hour)
            // {
            //     if (sta == 0)
            //     {
            //         inter.SetOperator(_kktModel.CashierName, _kktModel.OperatorInn);
            //         var openShiftsAnswer = inter.OpenShift();
            //         return;
            //     }
            // }
            // else
            // {
            //     if (sta == 1) inter.CloseShift();
            // }
            
            
            
            var k = new KassaManager();

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
            
            k.StartKassa();
            k.SuccessfullyReceipt += (sender, eventArgs) =>
            {
                // var logBitmap = (Bitmap)Image.FromFile("Resources/Images/Logo.jpg");
                // logBitmap.Save("Logo.png");
                // var logo = ImageHelper.CenterAlign(logBitmap);
                var logo = File.ReadAllBytes("Resources/Images/Logo.jpg");
                k.PrintUsersDocument(new List<TicketModel>()
                {
                    new TicketModel()
                    {
                        Logo = logo,
                        Address = "Улица свободы 20",
                        DateTime = "22.10.2022 14:00",
                        Places = "Ряд 12 место 10",
                        Name = "Золотой ключик"
                    },
                    new TicketModel()
                    {
                        Logo = logo,
                        Address = "Улица свободы 20",
                        DateTime = "22.10.2022 14:00",
                        Places = "Ряд 12 место 11",
                        Name = "Золотой ключик"
                    },
                    new TicketModel()
                    {
                        Logo = logo,
                        Address = "Улица свободы 20",
                        DateTime = "22.10.2022 14:00",
                        Places = "Ряд 12 место 12",
                        Name = "Золотой ключик"
                    }
                });
            };
            k.RegisterReceipt(false, new ReceiptModel()
                {
                    isElectron = false,
                    TaxationType = TaxationTypeEnum.Osn,
                    TypeReceipt = TypeReceipt.Sell
                }, 
                new List<BasketModel>() 
                { 
                    new BasketModel() 
                    {
                        Cost = 10,
                        MeasurementUnit = MeasurementUnitEnum.Piece,
                        Name = "Фотографии",
                        PaymentObject = PaymentObjectEnum.Service,
                        Quantity = 5,
                        TaxType = TaxTypeEnum.Vat10
                    }
                },
                new PayModel()
                {
                    PaymentType = PaymentTypeEnum.cash,
                    Sum = 50
                }
            );
            
            Console.ReadKey();
        }
    }
    
}