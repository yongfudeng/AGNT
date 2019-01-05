using System.Collections.Generic;
using System.Text;

namespace BOC.SynchronousService.Framework
{
    public class Message
    {
        #region Fields

        private string _ID = string.Empty;

        private HashSet<FieldSchema> _Head = null;

        private HashSet<FieldSchema> _Body = null;

        #endregion

        #region Constructors

        public Message()
        {
            _Head = new HashSet<FieldSchema>();
            _Body = new HashSet<FieldSchema>();
        }

        public Message(Message message)
        {
            ID = message.ID;
            Encoding = message.Encoding;

            _Head = new HashSet<FieldSchema>();
            _Body = new HashSet<FieldSchema>();

            foreach (var h in message.Head)
            {
                _Head.Add(h.Clone());
            }
            foreach (var b in message.Body)
            {
                _Body.Add(b.Clone());
            }

        }
        #endregion

        #region Public Properties

        public string ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }
        public HashSet<FieldSchema> Head
        {
            get
            {
                return _Head;
            }
        }

        public HashSet<FieldSchema> Body
        {
            get
            {
                return _Body;
            }
        }
        public Encoding Encoding
        {
            get;
            set;
        } = Encoding.UTF8;

        #endregion

        #region Function

        public Message Clone()
        {
            return new Message(this);
        }

        #endregion

    }
}