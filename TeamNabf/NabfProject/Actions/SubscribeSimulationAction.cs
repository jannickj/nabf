using NabfProject.AI;
using NabfProject.SimManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XmasEngineModel.EntityLib;
using XmasEngineModel.Management;

namespace NabfProject.Actions
{
    public class SubscribeSimulationAction : EntityXmasAction<NabfAgent>
    {
        private int simID;

        public SubscribeSimulationAction(int simID)
        {
            this.simID = simID;
        }

        protected override void Execute()
        {
            SimulationManager simMan = ((NabfModel)this.Engine).SimulationManager;
            NabfAgent agent = this.Source;

            //DO FILTHY STUFF TO SIMULATION MANAGER!
            throw new NotImplementedException();
        }
    }
}
