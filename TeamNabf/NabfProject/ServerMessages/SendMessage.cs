using NabfProject.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.ServerMessages
{
    public class SendMessage : XmlTransmitterMessage<InternalSendMessage>
    {
        public override string NodeName
        {
            get
            {
                return "message";
            }
        }

        public SendMessage(InternalSendMessage iMessage)
            : base(iMessage)
        { }
    }
}
