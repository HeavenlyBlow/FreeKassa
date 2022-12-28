using System;
using System.Collections.Generic;
using AtolDriver;
using FreeKassa.Model;
using FreeKassa.Model.PrinitngDocumensModel;

namespace FreeKassa
{
    public class Test
    {
        public static void Main(string[] args)
        {
            
            
            
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
            // while (true)
            // {
            //     Console.WriteLine("+");
            // }
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
            k.PrintUsersDocument(new List<TicketModel>()
            {
                new TicketModel()
                {
                    Address = "Улица свободы 20",
                    DateTime = "22.10.2022 14:00",
                    Places = "Ряд 12 место 10",
                    Name = "Золотой ключик"
                },
                new TicketModel()
                {
                    Address = "Улица свободы 20",
                    DateTime = "22.10.2022 14:00",
                    Places = "Ряд 12 место 11",
                    Name = "Золотой ключик"
                },
                new TicketModel()
                {
                    Address = "Улица свободы 20",
                    DateTime = "22.10.2022 14:00",
                    Places = "Ряд 12 место 12",
                    Name = "Золотой ключик"
                }
            });

            Console.ReadKey();
        }
    }
}