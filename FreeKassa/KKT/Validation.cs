using System.ComponentModel.Design;

namespace FreeKassa.KKT
{
    public static class Validation
    {
        private static int _lastFiscalDocumentNumber;
        public static bool CheckSetting(Model.KKT settings)
        {
            return settings.OperatorName != "" &&
                   (settings.Shift.NonStopWork.On != 1 || settings.Shift.WorkKWithBreaks.On != 1);
        }
        public static void SetLastFiscalDocumentNumber(int receiptNumber)
        {
            _lastFiscalDocumentNumber = receiptNumber;
        }
    }
}