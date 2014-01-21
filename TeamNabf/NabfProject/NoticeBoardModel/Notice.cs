using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NabfProject.AI;
using NabfProject.KnowledgeManagerModel;

namespace NabfProject.NoticeBoardModel
{
    public abstract class Notice : IEquatable<Notice>, IComparable
    {
        public List<NodeKnowledge> WhichNodes { get; protected set; }
        public int AgentsNeeded { get; protected set; }
        public Int64 Id { get; private set; }
        public int Value { get; protected set; }

        private int _highestAverageDesirabilityForNotice = -1;
        public int HighestAverageDesirabilityForNotice { get { return _highestAverageDesirabilityForNotice; } set { _highestAverageDesirabilityForNotice = value; } }
        private List<NabfAgent> _agentsApplied = new List<NabfAgent>();
        private List<NabfAgent> _topDesireAgents = new List<NabfAgent>();
        private Dictionary<NabfAgent, int> _agentsToDesirability = new Dictionary<NabfAgent, int>(); 

        public Notice(Int64 id)
        {
            Id = id;
        }

        public List<NabfAgent> GetTopDesireAgents()
        {
            return this._topDesireAgents.ToList();
        }

        public void AddToTopDesireAgents(NabfAgent toAdd)
        {
            _topDesireAgents.Add(toAdd);
        }

        public void AddRangeToTopDesireAgents(ICollection<NabfAgent> toAdd)
        {
            _topDesireAgents.AddRange(toAdd);
        }

        public List<NabfAgent> GetAgentsApplied()
        {
            return _agentsApplied.ToList();
        }

        public bool TryGetValueAgentToDesirabilityMap(NabfAgent agent, out int desire)
        {
            return _agentsToDesirability.TryGetValue(agent, out desire);
        }

        public bool ContentIsEqualTo(Notice no)
        {
            if (no == null)
                throw new ArgumentException("Input of ContentIsEqualTo of " + this.GetType().Name + " is null");
            else if (!(no is Notice))
                throw new ArgumentException("Object : " + no.GetType().Name + " of ContentIsEqualTo is not of type Notice");

            if (no.GetType() != this.GetType())
                return false;
            else
                if (no.WhichNodes.Except<NodeKnowledge>(this.WhichNodes).Count() != 0)
                    return false;

            return no.AgentsNeeded == this.AgentsNeeded && no.Value == this.Value;
        }

        public void Apply(int desirability, NabfAgent a)
        {
            _agentsToDesirability.Add(a, desirability);
            _agentsApplied.Add(a);
        }
        public void UnApply(NabfAgent a, NoticeBoard nb)
        {
            int des = -2;
            _agentsToDesirability.TryGetValue(a, out des);
            _agentsToDesirability.Remove(a);
            _agentsApplied.Remove(a);
            bool b = _topDesireAgents.Remove(a);
            int lowestDesire;
            SortedList<int, NabfAgent> topDesires;
            List<NabfAgent> agentsToAdd;
            if (b)
            {
                lowestDesire = nb.FindTopDesiresForNotice(this, out topDesires, out agentsToAdd);
                if (lowestDesire != -1)
                {
                    b = nb.RemoveJob(this);
                    if (b)
                    {
                        _topDesireAgents.Clear();
                        _topDesireAgents.AddRange(agentsToAdd);
                        HighestAverageDesirabilityForNotice = topDesires.Keys.Sum() / topDesires.Keys.Count;
                        nb.AddJob(this);
                    }
                }
                else
                    nb.RemoveJob(this);
            }
        }

        public void UpdateNotice(List<NodeKnowledge> whichNodes, int agentsNeeded, int value)
        {
            WhichNodes = whichNodes;
            AgentsNeeded = agentsNeeded;
            Value = value;
        }

        bool IEquatable<Notice>.Equals(Notice other)
        {
            if (other == null)
                return false;
            if (!(other is Notice))
                return false;
            return Id == other.Id;
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
                throw new ArgumentException("Input of CompareTo of " + this.GetType().Name + " is null");
            else if (obj is Notice)
                if (((Notice)obj).HighestAverageDesirabilityForNotice < HighestAverageDesirabilityForNotice)
                    return -1;
                else if (((Notice)obj).HighestAverageDesirabilityForNotice > HighestAverageDesirabilityForNotice)
                    return 1;
                else
                    return 0;
            else
                throw new ArgumentException("Object : " + obj.GetType().Name + " of CompareTo is not of type Notice");
        }
    }
    
    public class DisruptJob : Notice
    {

        public DisruptJob(int agentsNeeded, List<NodeKnowledge> whichNodes, int value, Int64 id)
            : base(id)
        {
            AgentsNeeded = agentsNeeded;
            WhichNodes = whichNodes;
            Value = value;
        }
    }

    public class AttackJob : Notice
    {

        public AttackJob(int agentsNeeded, List<NodeKnowledge> whichNodes, int value, Int64 id)
            : base(id)
        {
            AgentsNeeded = agentsNeeded;
            WhichNodes = whichNodes;
            Value = value;
        }
    }

    public class OccupyJob : Notice
    {

        public OccupyJob(int agentsNeeded, List<NodeKnowledge> whichNodes, int value, Int64 id)
            : base(id)
        {
            AgentsNeeded = agentsNeeded;
            WhichNodes = whichNodes;
            Value = value;
        }
    }

    public class RepairJob : Notice
    {

        public RepairJob(List<NodeKnowledge> whichNodes, int value, Int64 id)
            : base(id)
        {
            WhichNodes = whichNodes;
            AgentsNeeded = 1;
            Value = value;
        }
    }


}
