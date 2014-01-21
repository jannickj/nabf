using JSLibrary.Conversion;
using JSLibrary.IiLang.DataContainers;
using NabfProject.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.Parsers.AgentToAgentMasterConverters
{
    public class ConverterUnapplyNotice: JSConverterToForeign<UnapplyNoticeAction, IilAction>
    {
        public override IilAction BeginConversionToForeign(UnapplyNoticeAction gobj)
        {
            throw new NotImplementedException();
        }
    }
}
