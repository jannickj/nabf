using NabfProject.AI;
using NabfProject.SimManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XmasEngineModel.EntityLib;
using XmasEngineModel.Management;
using NabfProject.KnowledgeManagerModel;
using NabfProject.NoticeBoardModel;

namespace NabfProject.Actions
{
    public class CreateNoticeAction : EntityXmasAction<NabfAgent>
    {
        private int SimId;
        private NoticeBoardModel.NoticeBoard.JobType JobType;
        private int AgentsNeeded;
        private List<NodeKnowledge> WhichNodes;
        private int Value;

        public CreateNoticeAction(int simID, NoticeBoard.JobType jobType, int agentsNeeded, List<NodeKnowledge> whichNodes, int value)
        {
            SimId = simID;
            JobType = jobType;
            AgentsNeeded = agentsNeeded;
            WhichNodes = whichNodes;
            Value = value;
        }
        protected override void Execute()
        {
            SimulationManager simMan = ((NabfModel)this.Engine).SimulationManager;

            Notice n;
            simMan.CreateAndAddNotice(SimId, JobType, AgentsNeeded, WhichNodes, Value, out n);
        }
    }
}
