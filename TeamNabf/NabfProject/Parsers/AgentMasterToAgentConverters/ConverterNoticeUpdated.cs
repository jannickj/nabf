using JSLibrary.Conversion;
using JSLibrary.IiLang;
using NabfProject.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.Parsers.AgentMasterToAgentConverters
{
    public class ConverterNoticeUpdated : JSConverterToForeign<NoticeUpdatedEvent, IilPerceptCollection>
    {
        public override IilPerceptCollection BeginConversionToForeign(NoticeUpdatedEvent gobj)
        {
            throw new NotImplementedException();
        }
    }
}
