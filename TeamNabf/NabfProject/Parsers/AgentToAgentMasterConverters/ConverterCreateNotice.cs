using JSLibrary.Conversion;
using JSLibrary.IiLang.DataContainers;
using NabfProject.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.Parsers.AgentToAgentMasterConverters
{
    public class ConverterCreateNotice : JSConverterToForeign<IilAction, CreateNoticeAction>
    {
        public override object KnownID
        {
            get
            {
                return base.KnownID;
            }
        }


        public override CreateNoticeAction BeginConversionToForeign(IilAction gobj)
        {
            throw new NotImplementedException();
        }
    }
}
