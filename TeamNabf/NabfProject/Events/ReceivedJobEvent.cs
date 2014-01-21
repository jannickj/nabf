using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NabfProject.NoticeBoardModel;
using XmasEngineModel.Management;

namespace NabfProject.Events
{
    public class ReceivedJobEvent : XmasEvent
    {
        public Notice Notice { get; private set; }

        public ReceivedJobEvent(Notice n)
        {
            Notice = n;
        }
    }
}
