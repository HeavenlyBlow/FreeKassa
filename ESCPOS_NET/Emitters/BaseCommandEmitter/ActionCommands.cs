using ESCPOS_NET.Emitters.BaseCommandValues;

namespace ESCPOS_NET.Emitters
{
    public abstract partial class BaseCommandEmitter : ICommandEmitter
    {
        /* Action Commands */
        public virtual byte[] FullCut() => new byte[] { Cmd.GS, Ops.PaperCut, Functions.PaperCutFullCut };

        public virtual byte[] PartialCut() => new byte[] { Cmd.GS, Ops.PaperCut, Functions.PaperCutPartialCut };

        public virtual byte[] FullCutAfterFeed(int lineCount) => new byte[] { Cmd.GS, Ops.PaperCut, Functions.PaperCutFullCutWithFeed, (byte)lineCount };

        public virtual byte[] PartialCutAfterFeed(int lineCount) => new byte[] { Cmd.GS, Ops.PaperCut, Functions.PaperCutPartialCutWithFeed, (byte)lineCount };
        public virtual byte[] EjectPaperAfterCut() => new byte[] {Cmd.GS, Functions.EjectPaper, (byte)0x05};
        public virtual byte[] PresentPaper() => new[] { Cmd.GS, Functions.EjectPaper, (byte)0x03, (byte)0x01 };
    }
}
