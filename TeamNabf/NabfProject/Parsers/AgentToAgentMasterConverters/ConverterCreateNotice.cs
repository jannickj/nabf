using JSLibrary.Conversion;
using JSLibrary.IiLang.DataContainers;
using NabfProject.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSLibrary.IiLang.Parameters;
using NabfProject.KnowledgeManagerModel;

namespace NabfProject.Parsers.AgentToAgentMasterConverters
{
    public class ConverterCreateNotice : JSConverterToForeign<IilAction, CreateNoticeAction>
    {
        public AgentMasterDataParsers MasterDataParser { get; set; }

        public override object KnownID
        {
            get
            {
                return "createNoticeAction";
            }
        }


        public override CreateNoticeAction BeginConversionToForeign(IilAction gobj)
        {
            int simId = (int)((IilNumeral)gobj.Parameters[0]).Value;

            int jobType = (int)((IilNumeral)gobj.Parameters[1]).Value;

            int agentsNeeded = (int)((IilNumeral)gobj.Parameters[2]).Value;

            int value = (int)((IilNumeral)gobj.Parameters[4]).Value;

            List<NodeKnowledge> nodes = ((IilFunction)gobj.Parameters[3]).Parameters
                .Select(k => (NodeKnowledge)MasterDataParser.ConvertToKnown(k)).ToList();

            CreateNoticeAction cna = new CreateNoticeAction(simId, (NoticeBoardModel.NoticeBoard.JobType)jobType, agentsNeeded, nodes, value);

            return cna;
        }
    }
}
