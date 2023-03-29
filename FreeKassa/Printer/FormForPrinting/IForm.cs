using ESCPOS_NET.Emitters;

namespace FreeKassa.Printer.FormForPrinting
{
    public interface IForm
    {
        /// <summary>
        /// Внутри метода необходимо релизовать метод ByteSplicer.Combine для возрващнеия массива байт для печати
        /// </summary>
        /// <param name="e">Объект принтера: можно создать новый</param>
        /// <param name="obj">Модель данных</param>
        /// <returns></returns>
        public byte[] GetFormData(EPSON e, object obj);
    }
}