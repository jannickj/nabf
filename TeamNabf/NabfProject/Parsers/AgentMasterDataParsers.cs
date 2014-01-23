using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSLibrary.Conversion;
using JSLibrary.IiLang;
using JSLibrary.IiLang.DataContainers;
using JSLibrary.IiLang.Parameters;
using NabfProject.Parsers.AgentToMarsConverters;
using NabfProject.ServerMessages;

namespace NabfProject.Parsers
{
	public class AgentMasterDataParsers : JSConversionTool<object,IilElement>
	{
        public AgentMasterDataParsers()
		{
			this.IdOfForeign = new JSConversionIDFetcherSimple<IilElement>(fetchId);
            this.AddConverter(new KnowledgeConverters.EdgeKnowledgeConverter());
            this.AddConverter(new KnowledgeConverters.NodeKnowledgeConverter());
            this.AddConverter(new KnowledgeConverters.RoleKnowledgeConverter());
            this.AddConverter(new NoticeConverters.NoticeConverter());
		}

		private object fetchId(IilElement element)
		{
            var ipc = (IilFunction)element;

            return ipc.Name;
		}

	}
}
