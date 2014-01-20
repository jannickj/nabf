using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.NoticeBoardModel
{
    public class NewNoticeEvent : XmasEngineModel.Management.XmasEvent
    {
        public Notice Notice { get; private set; }

        public NewNoticeEvent(Notice n)
        {
            Notice = n;
        }
    }
}
