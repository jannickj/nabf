using NabfProject.AI;
using NabfProject.SimManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XmasEngineModel.EntityLib;
using XmasEngineModel.Management;
using NabfProject.NoticeBoardModel;

namespace NabfProject.Actions
{
    public class UnapplyNoticeAction : EntityXmasAction<NabfAgent>
    {
        private int SimId;
        private Notice Notice;

        public UnapplyNoticeAction(int simID, Notice notice)
        {
            SimId = simID;
            Notice = notice;
        }
    
        protected override void Execute()
        {
            SimulationManager simMan = ((NabfModel)this.Engine).SimulationManager;

            simMan.UnApplyToNotice(SimId, Notice, this.Source);
        }
    }
}
