using JSLibrary.Conversion;
using JSLibrary.IiLang.DataContainers;
using NabfProject.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.Parsers.AgentToAgentMasterConverters
{
    public class ConverterUnapplyNotice : JSConverterToForeign<IilAction, UnapplyNoticeAction>
    {
        public override object KnownID
        {
            get
            {
                return base.KnownID;
            }
        }

        public override IilAction BeginConversionToForeign(UnapplyNoticeAction gobj)
        {
            throw new NotImplementedException();
        }
    }
}
