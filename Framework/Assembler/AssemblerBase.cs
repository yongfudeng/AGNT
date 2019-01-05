using BOC.ChannelInterface.Component.Encodings;
using System;
using System.Text;
namespace BOC.SynchronousService.Framework.Assembler
{
    public abstract class AssemblerBase
    {
        #region Fields

        protected string _EncodingName = string.Empty;
        protected Encoding _Encoding = System.Text.Encoding.UTF8;

        #endregion

        #region Constructors

        protected AssemblerBase()
        {
        }

        #endregion

        #region Public Properties

        public string ID
        {
            get;
            set;
        } = string.Empty;

        public string Encoding
        {
            get
            {
                return _EncodingName;
            }
            set
            {
                _EncodingName = value;
                switch (value)
                {
                    case "IBM-1388":
                        _Encoding = IBM1388Encoding.Encoding;
                        break;
                    default:
                        try
                        {
                            _Encoding = System.Text.Encoding.GetEncoding(value.Trim());
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"编码转换失败.{value}.{e}");
                        }
                        break;
                }
            }
        }
        public Encoding Encode
        {
            get
            {
                return _Encoding;
            }
        }

        #endregion

        #region Methods


        public abstract void Assemble(Message context, bool isHeader = false);

        public abstract void Dissemble(byte[] bytes, Message context, bool isHeader = false);

        #endregion

    }
}