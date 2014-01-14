using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public void Subscribe(NabfAgent agent)
        {
            _sharingList.Add(agent);
        }
        public bool Unsubscribe(NabfAgent agent)
        {
            return _sharingList.Remove(agent);
        }

        public void SendPercept(IilFunction iilfunc, NabfAgent agent)
        {
            foreach (IilParameter param in iilfunc.Parameters)
            {

            }
        }
    }

    public interface Knowledge : IEquatable<Knowledge>
    {

    }

}
