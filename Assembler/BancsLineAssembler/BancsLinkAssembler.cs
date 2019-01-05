using System;
using BOC.SynchronousService.Framework.Assembler;
using BOC.SynchronousService.Framework;

namespace BOC.SynchronousService.Assembler
{
    /// <summary>
    /// 文件解析器
    /// </summary>
    public sealed class BancsLinkAssembler : AssemblerBase
    {
        public override void Assemble(Message context, bool isHeader = false)
        {
            if (isHeader)
                foreach (var f in context.Head)
                {
                    f.Encoding = _Encoding;
                    f.Assemble();
                }
            else
                foreach (var f in context.Body)
                {
                    f.Encoding = _Encoding;
                    f.Assemble();
                }
        }

        public override void Dissemble(byte[] bytes, Message context, bool isHeader = false)
        {
            int position = 0;
            if (isHeader)
                foreach (var ch in context.Head)
                {
                    var b = new byte[ch.Length];
                    Buffer.BlockCopy(bytes, position, b, 0, b.Length);
                    ch.Encoding = _Encoding;
                    ch.Disassemble(b);
                    position += b.Length;
                }
            else
                foreach (var cb in context.Body)
                {
                    var b = new byte[cb.Length];
                    Buffer.BlockCopy(bytes, position, b, 0, b.Length);
                    cb.Encoding = _Encoding;
                    cb.Disassemble(b);
                    position += b.Length;
                }

        }
    }
}