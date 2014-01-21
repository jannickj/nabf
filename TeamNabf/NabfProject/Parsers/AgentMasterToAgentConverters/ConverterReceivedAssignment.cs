using JSLibrary.Conversion;
using JSLibrary.IiLang;
using NabfProject.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.Parsers.AgentMasterToAgentConverters
{
    public class ConverterReceivedAssignment : JSConverterToForeign<ReceivedJobEvent, IilPerceptCollection>
    {
        public override IilPerceptCollection BeginConversionToForeign(ReceivedJobEvent gobj)
        {
            throw new NotImplementedException();
        }
    }
}
