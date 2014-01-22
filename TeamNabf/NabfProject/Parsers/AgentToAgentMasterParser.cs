using JSLibrary.Conversion;
using JSLibrary.IiLang.DataContainers;
using NabfProject.AI;
using NabfProject.Parsers.AgentToAgentMasterConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XmasEngineModel.Management;

namespace NabfProject.Parsers
{
    public class AgentToAgentMasterParser : JSConversionTool<IilAction, EntityXmasAction<NabfAgent>>
    {
        public AgentToAgentMasterParser()
        {
            this.IdOfKnown = new JSConversionIDFetcherSimple<IilAction>(GetIdFromIilAction);

            this.AddConverter(new ConverterAddKnowledge());
            this.AddConverter(new ConverterApplyNotice());
            this.AddConverter(new ConverterChangeNotice());
            this.AddConverter(new ConverterCreateNotice());
            this.AddConverter(new ConverterDeleteNotice());
            this.AddConverter(new ConverterUnapplyNotice());
        }

        private object GetIdFromIilAction(IilAction a)
        {
            return a.Name;
        }
    }
}
