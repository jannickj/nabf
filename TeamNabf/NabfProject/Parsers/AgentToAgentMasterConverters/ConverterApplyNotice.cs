using JSLibrary.Conversion;
using JSLibrary.IiLang.DataContainers;
using NabfProject.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.Parsers.AgentToAgentMasterConverters
{
    public class ConverterApplyNotice : JSConverterToForeign<IilAction, ApplyNoticeAction>
    {
        public override object KnownID
        {
            get
            {
                return base.KnownID;
            }
        }

        public override ApplyNoticeAction BeginConversionToForeign(IilAction gobj)
        {
            throw new NotImplementedException();
        }
    }
}
