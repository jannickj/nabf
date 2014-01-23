﻿using JSLibrary.Conversion;
using JSLibrary.IiLang.DataContainers;
using NabfProject.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSLibrary.IiLang.Parameters;

namespace NabfProject.Parsers.AgentToAgentMasterConverters
{
    public class ConverterApplyNotice : JSConverterToForeign<IilAction, ApplyNoticeAction>
    {
        public override object KnownID
        {
            get
            {
                return "applyNoticeAction";
            }
        }

        public override ApplyNoticeAction BeginConversionToForeign(IilAction gobj)
        {
            IilFunction bonusfunc = ((IilFunction)gobj.Parameters[0]);

            int simId = (int)((IilNumeral)bonusfunc.Parameters[0]).Value;

            int noticeId = (int)((IilNumeral)bonusfunc.Parameters[1]).Value;

            int desire = (int)((IilNumeral)bonusfunc.Parameters[2]).Value;            

            ApplyNoticeAction ana = new ApplyNoticeAction(simId, noticeId, desire);

            return ana;
        }
    }
}
