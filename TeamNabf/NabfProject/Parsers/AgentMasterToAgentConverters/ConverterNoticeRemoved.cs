using JSLibrary.Conversion;
using JSLibrary.IiLang;
using NabfProject.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.Parsers.AgentMasterToAgentConverters
{
    public class ConverterNoticeRemoved : JSConverterToForeign<NoticeRemovedEvent, IilPerceptCollection>
    {
        public override IilPerceptCollection BeginConversionToForeign(NoticeRemovedEvent gobj)
        {
            throw new NotImplementedException();
        }
    }
}
