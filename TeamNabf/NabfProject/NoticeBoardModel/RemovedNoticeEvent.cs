using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.NoticeBoardModel
{
    public class RemovedNoticeEvent : XmasEngineModel.Management.XmasEvent
    {
        public Notice Notice { get; private set; }

        public RemovedNoticeEvent(Notice n)
        {
            Notice = n;
        }
    }
}
