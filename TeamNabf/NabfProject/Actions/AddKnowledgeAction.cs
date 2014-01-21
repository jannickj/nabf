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
    public class AddKnowledgeAction : EntityXmasAction<NabfAgent>
    {


        protected override void Execute()
        {
            SimulationManager simMan = ((NabfModel)this.Engine).SimulationManager;

            //DO FILTHY STUFF TO SIMULATION MANAGER!
            throw new NotImplementedException();
        }
    }
}
