using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using ESCPOS_NET.ConsoleTest;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using FreeKassa.Model;
using FreeKassa.Model.FiscalDocumentsModel;
using FreeKassa.Model.PrinitngDocumensModel;

namespace FreeKassa.FormForPrinting.UsersDocument
{
    // vkp80ii.PrintImage(File.ReadAllBytes("Resources/Images/ArlikinLogo.png"), false),
    public class TiсketForm
    {
        public static byte[] GetTicketForm(EPSON vkp80ii, IEnumerable<TicketModel> tiket)
        {
            var data = ByteSplicer.Combine(
                vkp80ii.LeftAlign()
            );
            
            foreach (var model in tiket)
            {
                data = Combine(vkp80ii, model, data);
            }

            return data;
            // return ByteSplicer.Combine(
            //     vkp80ii.LeftAlign(),
            //     vkp80ii.PrintImage(tiket.Logo, false, isLegacy: true),
            //     vkp80ii.PrintLine(""),
            //     vkp80ii.PrintLine(""),
            //     vkp80ii.CenterAlign(),
            //     vkp80ii.PrintLine("Билет на представление"),
            //     vkp80ii.PrintLine(""),
            //     vkp80ii.PrintLine($"Название: {tiket.Name}"),
            //     vkp80ii.PrintLine(""),
            //     vkp80ii.PrintLine($"Места: {tiket.Places}"),
            //     vkp80ii.PrintLine(""), 
            //     vkp80ii.PrintLine($"Адрес: {tiket.Address}"),
            //     vkp80ii.PrintLine(""),
            //     vkp80ii.PrintLine($"Дата и время: {tiket.DateTime}"), 
            //     vkp80ii.PrintLine(""), 
            //     vkp80ii.PrintLine(""), 
            //     vkp80ii.PrintLine("")
            // );
        }

        private static byte[] Combine(EPSON vkp80ii ,TicketModel ticket, byte[] data)
        {
            return ByteSplicer.Combine(data,
                vkp80ii.LeftAlign(),
                vkp80ii.PrintImage(ticket.Logo, false, isLegacy: true),
                vkp80ii.PrintLine(""),
                vkp80ii.PrintLine(""),
                vkp80ii.CenterAlign(),
                vkp80ii.PrintLine("Билет на представление"),
                vkp80ii.PrintLine(""),
                vkp80ii.PrintLine($"Название: {ticket.Name}"),
                vkp80ii.PrintLine(""),
                vkp80ii.PrintLine($"Места: {ticket.Places}"),
                vkp80ii.PrintLine(""), 
                vkp80ii.PrintLine($"Адрес: {ticket.Address}"),
                vkp80ii.PrintLine(""),
                vkp80ii.PrintLine($"Дата и время: {ticket.DateTime}"), 
                vkp80ii.PrintLine(""),
                vkp80ii.EjectPaperAfterCut()
            );
        }
    }
}