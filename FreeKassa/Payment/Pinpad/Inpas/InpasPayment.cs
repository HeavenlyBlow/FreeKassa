using System;
using DualConnector;
using FreeKassa.Utils;

namespace FreeKassa.Payment.Pinpad.Inpas
{
    public class InpasPayment : PaymentBase, IDisposable
    {
        private SimpleLogger _logger;
        private DCLink _dcLink;
        
        public InpasPayment(SimpleLogger logger)
        {
            _logger = logger;
        }
        
        public void StartPayment(long amount, string terminalId)
        {
            _dcLink = new DualConnector.DCLink();
            ISAPacket query = new DualConnector.SAPacket();
            ISAPacket response = new DualConnector.SAPacket();
            query.Amount = amount.ToString();
            query.CurrencyCode = "643";
            query.OperationCode = 1;
            query.TerminalID = terminalId;
            _dcLink.OnExchange +=  new DualConnector.OnExchangeHandler(dclink_OnExchange);
            var res = _dcLink.InitResources();
            
            if (res != 0)
            {
                _logger.Error("InpasPayment: Ошибка инициализации ресурсов");
            }

            res = _dcLink.Exchange(ref query, ref response, 0);

        }

        private void dclink_OnExchange(int result)
        {
            
            _dcLink.Dispose();
        }

        public void Dispose()
        {
            _dcLink.Dispose();
        }
    }
}