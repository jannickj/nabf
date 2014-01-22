using JSLibrary.Conversion;
using JSLibrary.IiLang.DataContainers;
using NabfProject.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSLibrary.IiLang.Parameters;
using NabfProject.KnowledgeManagerModel;
using NabfProject.NoticeBoardModel;

namespace NabfProject.Parsers.AgentToAgentMasterConverters
{
    public class ConverterChangeNotice : JSConverterToForeign<IilAction, ChangeNoticeAction>
    {
        public AgentMasterDataParsers MasterDataParser { get; set; }

        public override object KnownID
        {
            get
            {
                return "changeNoticeAction";
            }
        }

        public override ChangeNoticeAction BeginConversionToForeign(IilAction gobj)
        {
            int simId = (int)((IilNumeral)gobj.Parameters[0]).Value;

            Int64 noticeId = (Int64)((IilNumeral)gobj.Parameters[1]).Value;

            int type = (int)((IilNumeral)gobj.Parameters[2]).Value;
            NoticeBoard.JobType jobType = (NoticeBoard.JobType)type;

            int agentsNeeded = (int)((IilNumeral)gobj.Parameters[3]).Value;

            List<NodeKnowledge> nodes = ((IilFunction)gobj.Parameters[4]).Parameters
                .Select(k => (NodeKnowledge)MasterDataParser.ConvertToKnown(k)).ToList();

            int value = (int)((IilNumeral)gobj.Parameters[5]).Value;

            List<NodeKnowledge> zone = new List<NodeKnowledge>();
            string agentToRepair = "";
            switch (jobType)
            {
                case NoticeBoard.JobType.Occupy:
                    zone = ((IilFunction)gobj.Parameters[6]).Parameters
                       .Select(k => (NodeKnowledge)MasterDataParser.ConvertToKnown(k)).ToList();
                    break;
                case NoticeBoard.JobType.Repair:
                    agentToRepair = ((IilIdentifier)gobj.Parameters[6]).Value;
                    break;
            }

            ChangeNoticeAction cna = new ChangeNoticeAction(simId, noticeId, agentsNeeded, nodes, zone, agentToRepair, value);

            return cna;
        }
    }
}
