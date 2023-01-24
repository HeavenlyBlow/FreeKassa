using System.ComponentModel.Design;

namespace FreeKassa.KKT
{
    public static class ValidationKktSettings
    {
        public static bool Check(Model.KKT settings)
        {
            return settings.OperatorName != "" &&
                   (settings.Shift.NonStopWork.On != 1 || settings.Shift.WorkKWithBreaks.On != 1);
        }
    }
}