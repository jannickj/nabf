﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSLibrary.Conversion;
using JSLibrary.IiLang.DataContainers;
using NabfProject.ServerMessages;

namespace NabfProject.Parsers
{
	public class AgentToMarsParser : JSConversionTool<IilAction,InternalSendMessage>
	{
		public AgentToMarsParser()
		{
			this.IdOfKnown = new JSConversionIDFetcherSimple<IilAction>(fetchId);
		}

		private object fetchId(IilAction action)
		{
			return action.Name;
		}

	}
}
