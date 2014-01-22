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
    public class ConverterAddKnowledge : JSConverterToForeign<IilAction, AddKnowledgeAction>
    {
        public override object KnownID
        {
            get
            {
                return "addKnowledgeAction";
            }
        }

        public override AddKnowledgeAction BeginConversionToForeign(IilAction gobj)
        {
            int simId = (int)((IilNumeral)gobj.Parameters[0]).Value;
            List<Knowledge> knowledge = new List<Knowledge>();


            AddKnowledgeAction aka = new AddKnowledgeAction(simId, knowledge);

            return aka;
        }
    }
}
