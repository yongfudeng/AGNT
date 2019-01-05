using System;
using System.Globalization;
using System.Text;
namespace BOC.SynchronousService.Framework
{
    public sealed class FieldSchema
    {
        #region Fields

        private string _Format = string.Empty;

        private int _Length = 0;

        private int _DecimalPointPosition = 0;

        #endregion

        #region Constructors

        public FieldSchema(string id, string format)
        {
            this.ID = id;
            this.Format = format;
        }

        public FieldSchema(FieldSchema schema)
        {
            this.ID = schema.ID;
            this.Format = schema.Format;
            this.Value = schema.Value;
            this.Encoding = schema.Encoding;
        }

        #endregion

        #region Public Properties

        public string ID
        {
            get;
            set;
        } = string.Empty;
        public string Format
        {
            get
            {
                return _Format;
            }
            private set
            {
                _Format = value;

                try
                {
                    int startIndex = 0, endIndex = 0;
                    while (endIndex < _Format.Length)
                    {
                        startIndex = value.IndexOf('(', endIndex + 1);
                        endIndex = value.IndexOf(')', endIndex + 1);
                        if (startIndex != -1 && endIndex != -1)
                        {
                            _Length += int.Parse(value.Substring(startIndex + 1, endIndex - startIndex - 1));
                        }
                        else
                        {
                            break;
                        }
                    }

                    int decimalPositionIndex = value.IndexOf('V');
                    if (decimalPositionIndex > -1)
                        _DecimalPointPosition = int.Parse(value.Substring(decimalPositionIndex).Replace("V9", "").Replace("(", "").Replace(")", ""));

                }
                catch (Exception e)
                {
                    throw new Exception($"格式设置错误.{this.ID}.{value}.{e.ToString()}");
                }
            }
        }
        public int Length
        {
            get
            {
                return _Length;
            }
        }
        public string Value
        {
            get;
            set;
        } = string.Empty;

        public Encoding Encoding
        {
            get;
            set;
        } = Encoding.UTF8;

        #endregion

        #region Function

        public void Assemble()
        {
            if (_Format.StartsWith("X"))
            {
                this.Value = $"{this.Value}{new string(' ', _Length - Encoding.GetBytes(this.Value).Length)}";
            }
            else if (_Format.StartsWith("9"))
            {
                Value = this.Value.Replace(".", "");
                Value = $"{new string('0', _Length - Encoding.GetBytes(this.Value).Length)}{this.Value}";
            }
        }

        public void Disassemble(byte[] b)
        {
            Value = Encoding.GetString(b);
            if (_Format.StartsWith("9"))
            {
                var value = Value.Insert(Value.Length - _DecimalPointPosition, CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                decimal number = 0;
                if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out number))
                    Value = number.ToString();
                else
                    throw new Exception($"返回的数据格式错误.{Value}");
            }
        }

        public FieldSchema Clone()
        {
            return new FieldSchema(this);
        }

        #endregion

    }
}