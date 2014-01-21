using JSLibrary.Conversion;
using JSLibrary.IiLang;
using NabfProject.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.Parsers.AgentMasterToAgentConverters
{
    public class ConverterNewNotice : JSConverterToForeign<NewNoticeEvent,IilPerceptCollection>
    {
        public override IilPerceptCollection BeginConversionToForeign(NewNoticeEvent gobj)
        {
            throw new NotImplementedException();
        }
    }
}
