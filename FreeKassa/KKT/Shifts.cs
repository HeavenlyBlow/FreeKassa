namespace FreeKassa.KKT
{
    public static class Shifts
    {
        private bool OpenShifts(Interfa)
        {
            if (_kktInterface.GetShiftStatus().Equals("1"))
            {
                if (CloseShifts() == 1)
                {
                    throw new ShiftException(_kktInterface.ReadError());
                }
            }

            if (_kktInterface.OpenShift() == 1)
            {
                throw new ShiftException(_kktInterface.ReadError());
            }
            return true;
        }
    }
}