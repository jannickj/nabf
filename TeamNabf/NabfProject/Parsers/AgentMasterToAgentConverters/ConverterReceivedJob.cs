using JSLibrary.Conversion;
using JSLibrary.IiLang;
using NabfProject.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSLibrary.IiLang.DataContainers;
using JSLibrary.IiLang.Parameters;

namespace NabfProject.Parsers.AgentMasterToAgentConverters
{
    public class ConverterReceivedJob : JSConverterToForeign<ReceivedJobEvent, IilPerceptCollection>
    {
        public AgentMasterDataParsers Parsers { get; set; }

        public override IilPerceptCollection BeginConversionToForeign(ReceivedJobEvent gobj)
        {
            IilPerceptCollection ipc;
            IilPercept percept = ((IilPerceptCollection)Parsers.ConvertToForeign(gobj.Notice)).Percepts[0];

            ipc = new IilPerceptCollection
            (
                new IilPercept("receivedJob"),
                new IilPercept("whichNodeIndexToGoTo", new IilNumeral(gobj.Notice.GetTopDesireAgents().IndexOf(gobj.Receiver))),
                percept
            );

            return ipc;
        }
    }
}
