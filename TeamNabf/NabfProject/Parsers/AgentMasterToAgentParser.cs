using JSLibrary.Conversion;
using JSLibrary.IiLang;
using NabfProject.Parsers.AgentMasterToAgentConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XmasEngineModel.Management;

namespace NabfProject.Parsers
{
    public class AgentMasterToAgentParser : JSConversionTool<XmasEvent, IilPerceptCollection>
    {

        public AgentMasterToAgentParser()
        {

            this.AddConverter(new ConverterReceivedAssignment());
            this.AddConverter(new ConverterNewKnowledge());
            this.AddConverter(new ConverterNewNotice());
            this.AddConverter(new ConverterNoticeRemoved());
            this.AddConverter(new ConverterNoticeUpdated());

        }

    }
}
