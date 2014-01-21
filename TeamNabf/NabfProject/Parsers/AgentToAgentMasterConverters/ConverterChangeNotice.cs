using JSLibrary.Conversion;
using JSLibrary.IiLang.DataContainers;
using NabfProject.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.Parsers.AgentToAgentMasterConverters
{
    public class ConverterChangeNotice : JSConverterToForeign<ChangeNoticeAction,IilAction>
    {
        public override IilAction BeginConversionToForeign(ChangeNoticeAction gobj)
        {
            throw new NotImplementedException();
        }
    }
}
