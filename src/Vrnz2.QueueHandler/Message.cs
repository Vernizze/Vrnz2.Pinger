using System;

namespace Vrnz2.QueueHandler
{
    public class Message<T>
    {
        #region Construtors

        public Message(T body)
        {
            Id = Guid.NewGuid();

            Body = body;

            CreationDate = DateTime.UtcNow;
        }

        #endregion


        #region Attributes

        public Guid Id { get; private set; } = Guid.Empty;

        public DateTime CreationDate { get; private set; } = DateTime.MinValue;

        public T Body { get; private set; } = default(T);

        #endregion
    }
}
