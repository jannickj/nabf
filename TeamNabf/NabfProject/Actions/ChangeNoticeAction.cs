﻿using NabfProject.AI;
using NabfProject.SimManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XmasEngineModel.EntityLib;
using XmasEngineModel.Management;
using NabfProject.KnowledgeManagerModel;

namespace NabfProject.Actions
{
    public class ChangeNoticeAction : EntityXmasAction<NabfAgent>
    {
        private int SimId;
        private Int64 NoticeId;
        private int AgentsNeeded;
        private List<NodeKnowledge> WhichNodes;
        private int Value;

        public ChangeNoticeAction(int simID, Int64 noticeID, int agentsNeeded, List<NodeKnowledge> whichNodes, int value)
        {
            SimId = simID;
            NoticeId = noticeID;
            AgentsNeeded = agentsNeeded;
            WhichNodes = whichNodes;
            Value = value;
        }

        protected override void Execute()
        {
            SimulationManager simMan = ((NabfModel)this.Engine).SimulationManager;

            simMan.UpdateNotice(SimId, NoticeId, AgentsNeeded, WhichNodes, Value);
        }
    }
}
