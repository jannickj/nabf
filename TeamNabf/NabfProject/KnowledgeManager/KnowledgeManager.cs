using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSLibrary.Data;
using JSLibrary.IiLang;
using JSLibrary.IiLang.Parameters;
using NabfProject;
using NabfProject.AI;

namespace NabfProject.KnowledgeManager
{
    public class KnowledgeManager
    {
        private List<NabfAgent> _sharingList = new List<NabfAgent>();

        private List<Knowledge> _knowledgeBase = new List<Knowledge>();

        private Dictionary<Knowledge, NabfAgent> _knowledgeToAgent = new Dictionary<Knowledge, NabfAgent>();
        private DictionaryList<NabfAgent, Knowledge> _agentToKnowledge = new DictionaryList<NabfAgent, Knowledge>();

        public void Subscribe(NabfAgent agent)
        {
            _sharingList.Add(agent);
        }
        public bool Unsubscribe(NabfAgent agent)
        {
            return _sharingList.Remove(agent);
        }

        public void SendKnowledge(List<Knowledge> knowledge, NabfAgent agent)
        {
            Knowledge kl;
            foreach (Knowledge k in knowledge)
            {
                if (!_knowledgeBase.Contains(k))
                {
                    _knowledgeBase.Add(k);
                    _knowledgeToAgent.Add(k, agent);
                    _agentToKnowledge.Add(agent, k);
                }
                else
                {
                    kl = _knowledgeBase.Find(pk => k.Equals(pk));
                    _knowledgeToAgent.Add(kl, agent);
                    _agentToKnowledge.Add(agent, kl);
                }
            }
        }
    }


}
