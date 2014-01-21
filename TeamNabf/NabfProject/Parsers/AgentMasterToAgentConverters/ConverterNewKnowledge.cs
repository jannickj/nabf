using JSLibrary.Conversion;
using JSLibrary.IiLang;
using NabfProject.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.Parsers.AgentMasterToAgentConverters
{
    public class ConverterNewKnowledge : JSConverterToForeign<NewKnowledgeEvent, IilPerceptCollection>
    {
        public override IilPerceptCollection BeginConversionToForeign(NewKnowledgeEvent gobj)
        {
            throw new NotImplementedException();
        }
    }
}
