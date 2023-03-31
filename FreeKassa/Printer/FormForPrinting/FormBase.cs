using ESCPOS_NET.Emitters;

namespace FreeKassa.Printer.FormForPrinting
{
    public abstract class FormBase
    {

        public byte[] Data;
        /// <summary>
        /// Внутри метода необходимо релизовать метод ByteSplicer.Combine для возрващнеия массива байт для печати
        /// </summary>
        /// <param name="e">Объект принтера: можно создать новый</param>
        /// <param name="obj">Модель данных</param>
        /// <returns></returns>
        public abstract byte[] GetFormData(EPSON e, object obj);
    }
}