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
            //IilPercept percept = ((IilPerceptCollection)Parsers.ConvertToForeign(gobj.Notice)).Percepts[0];

            int receiverIndex = gobj.Notice.GetTopDesireAgents().IndexOf(gobj.Receiver);

            ipc = new IilPerceptCollection
            (
                new IilPercept("receivedJob"),
                new IilPercept("noticeId", new IilNumeral(gobj.Notice.Id));
                new IilPercept("whichNodeNameToGoTo", new IilIdentifier(gobj.Notice.WhichNodes[receiverIndex].Name)),
                //percept
            );

            return ipc;
        }
    }
}
