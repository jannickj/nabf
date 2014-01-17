using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSLibrary.Conversion;
using JSLibrary.IiLang;
using NabfProject.ServerMessages;

namespace NabfProject.Parsers
{
	public class MarsToAgentParser : JSConversionTool<ReceiveMessage,IilPerceptCollection>
	{
		public MarsToAgentParser()
		{
			this.IdOfKnown = new JSConversionIDFetcherSimple<ReceiveMessage>(fetchId);
		}

		private object fetchId(ReceiveMessage message)
		{
			return message.Message.GetType();
		}
	}
}
