using System.Linq;

namespace ESCPOS_NET.Utils
{
    public class IdentHelper
    {
        /// <summary>
        /// Размещение строк в разных концах строки
        /// </summary>
        /// <param name="leftString">левая строка</param>
        /// <param name="rightString">правая строка</param>
        /// <param name="eStyle">тип шрифта</param>
        /// <returns></returns>
        public static string ArrangeWords(string leftString, string rightString, Style eStyle)
        {
            var indent = (int)eStyle;
            var resIter = 0;
            var result = new char[indent];
            var charLeftArray = leftString.ToCharArray();
            var charRightArray = rightString.ToCharArray();

            for (var i = 0; i < charLeftArray.Length; i++)
            {
                if (resIter <= indent - 1)
                {
                    result[i] = charLeftArray[i];
                    resIter++;
                }
            }

            for (var i = charRightArray.Length - 1; i >= 0 ; i--)
            {
                if (resIter != indent - 1)
                {
                    result[indent - 1] = charRightArray[i];
                    indent--;
                }
            }

            for (int i = 0; i < result.Length; i++)
            {
                var el = result[i];
                if (el.Equals('\0')) result[i] = ' ';
            }

            return new string(result);
        }
        
        public static string SolidLine(Style style) => new string(Enumerable.Repeat('-', (int)style).ToArray());

        public enum Style
        {
            FontB = 56,
            Default = 41,
            Bold = 55,
        }
    }
    
}