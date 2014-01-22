﻿using JSLibrary.Conversion;
using JSLibrary.IiLang;
using NabfProject.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.Parsers.AgentMasterToAgentConverters
{
    public class ConverterReceivedAssignment : JSConverterToForeign<ReceivedAssignmentEvent, IilPerceptCollection>
    {
        public override IilPerceptCollection BeginConversionToForeign(ReceivedAssignmentEvent gobj)
        {
            throw new NotImplementedException();
        }
    }
}