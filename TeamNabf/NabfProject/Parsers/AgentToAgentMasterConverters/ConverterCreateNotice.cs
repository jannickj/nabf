using JSLibrary.Conversion;
using JSLibrary.IiLang.DataContainers;
using NabfProject.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.Parsers.AgentToAgentMasterConverters
{
    public class ConverterCreateNotice : JSConverterToForeign<CreateNoticeAction, IilAction>
    {
        public override IilAction BeginConversionToForeign(CreateNoticeAction gobj)
        {
            throw new NotImplementedException();
        }
    }
}
