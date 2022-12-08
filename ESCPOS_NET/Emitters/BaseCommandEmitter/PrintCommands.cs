using System.Linq;
using System.Text;
using ESCPOS_NET.Emitters.BaseCommandValues;

namespace ESCPOS_NET.Emitters
{
    public abstract partial class BaseCommandEmitter : ICommandEmitter
    {
        /* Printing Commands */
        public virtual byte[] Print(string data)
        {
            //TODO Вот сдесь
            byte[] s = System.Text.Encoding.UTF8.GetBytes(data);
            // System.Text.Encoding.GetEncoding(1251).GetBytes(data);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var encode = Encoding.GetEncoding(866);
            var newbyte = Encoding.Convert(Encoding.UTF8, encode, s);
            // TODO: Sanitize...
            // return data.ToCharArray().Select(x => (byte)x).ToArray();
            return newbyte;
        }

        public virtual byte[] PrintLine(string line)
        {
            if (line == null)
            {
                return Print("\n");
            }

            return Print(line.Replace("\r", string.Empty).Replace("\n", string.Empty) + "\n");
        }

        public virtual byte[] FeedLines(int lineCount) => new byte[] { Cmd.ESC, Whitespace.FeedLines, (byte)lineCount };

        public virtual byte[] FeedLinesReverse(int lineCount) => new byte[] { Cmd.ESC, Whitespace.FeedLinesReverse, (byte)lineCount };

        public virtual byte[] FeedDots(int dotCount) => new byte[] { Cmd.ESC, Whitespace.FeedDots, (byte)dotCount };

        public virtual byte[] ReverseMode(bool enable) => new byte[] { Cmd.GS, Chars.ReversePrintMode, enable ? (byte)0x01 : (byte)0x00 };

        public virtual byte[] UpsideDownMode(bool enable) => new byte[] { Cmd.ESC, Chars.UpsideDownMode, enable ? (byte)0x01 : (byte)0x00 };
    }
}
