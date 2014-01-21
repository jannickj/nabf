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
    public class ApplyNoticeAction : EntityXmasAction<NabfAgent>
    {
        private int SimId;
        private Notice Notice;
        private int Desired;

        public ApplyNoticeAction(int simID, Notice notice, int desired)
        {
            SimId = simID;
            Notice = notice;
            Desired = desired;
        }

        protected override void Execute()
        {
            SimulationManager simMan = ((NabfModel)this.Engine).SimulationManager;

            simMan.ApplyToNotice(SimId, Notice, Desired, this.Source);
        }
    }
}
