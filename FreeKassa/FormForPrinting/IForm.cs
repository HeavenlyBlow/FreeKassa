using ESCPOS_NET.Emitters;

namespace FreeKassa.FormForPrinting
{
    public interface IForm
    {
        public byte[] GetByteForm(object model);
    }
}