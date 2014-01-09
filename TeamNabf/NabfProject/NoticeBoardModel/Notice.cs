using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NabfProject.AI;

namespace NabfProject.NoticeBoardModel
{
    public abstract class Notice : IComparable
    {
        public List<Node> WhichNodes { get; set; }
        public int AgentsNeeded { get; set; }

        public int HighestDesirabilityForNotice = -1;
        public List<NabfAgent> AgentsApplied = new List<NabfAgent>();
        public Dictionary<NabfAgent, int> AgentsToDesirability = new Dictionary<NabfAgent, int>(); 

        public Notice()
        {
        }

        public int CompareTo(object obj)
        {
            if (obj is Notice)
                if (((Notice)obj).HighestDesirabilityForNotice < HighestDesirabilityForNotice)
                    return -1;
                else if (((Notice)obj).HighestDesirabilityForNotice > HighestDesirabilityForNotice)
                    return 1;
                else
                    return 0;
            else if (obj == null)
                throw new ArgumentException("Input of CompareTo of " + this.GetType().Name + " is null");
            else
                throw new ArgumentException("Object : " + obj.GetType().Name + " of CompareTo is not of type Notice");
        }

        public bool ChildTypeIsEqualTo(Notice no)
        {
            if (no == null)
                throw new ArgumentException("Input of CompareTo of " + this.GetType().Name + " is null");
            else if (!(no is Notice))
                throw new ArgumentException("Object : " + no.GetType().Name + " of ChildTypeIsEqualTo is not of type Notice");

            if (no.GetType() != this.GetType())
                return false;
            else
                if (no.WhichNodes.Except<Node>(this.WhichNodes).Count() != 0)
                    return false;

            return no.AgentsNeeded == this.AgentsNeeded;
        }

        public void Apply(int desirability, NabfAgent a)
        {
            if (HighestDesirabilityForNotice < desirability)
                HighestDesirabilityForNotice = desirability;
            AgentsToDesirability.Add(a, desirability);
            AgentsApplied.Add(a);
        }
        public void UnApply(NabfAgent a)
        {
            int des = -2, newMaxDes = -1;
            AgentsToDesirability.TryGetValue(a, out des);
            AgentsToDesirability.Remove(a);
            AgentsApplied.Remove(a);
            if (HighestDesirabilityForNotice == des)
            {
                foreach (int i in AgentsToDesirability.Values)
                {
                    if (i > newMaxDes)
                        HighestDesirabilityForNotice = i;
                }
            }
        }

    }
    
    public class DisruptJob : Notice
    {

        public DisruptJob(int agentsNeeded, List<Node> whichNodes)
            : base()
        {
            AgentsNeeded = agentsNeeded;
            WhichNodes = whichNodes;
        }
    }

    public class AttackJob : Notice
    {

        public AttackJob(int agentsNeeded, List<Node> whichNodes)
            : base()
        {
            AgentsNeeded = agentsNeeded;
            WhichNodes = whichNodes;
        }
    }

    public class OccupyJob : Notice
    {

        public OccupyJob(int agentsNeeded, List<Node> whichNodes)
            : base()
        {
            AgentsNeeded = agentsNeeded;
            WhichNodes = whichNodes;
        }
    }

    public class RepairJob : Notice
    {

        public RepairJob(List<Node> whichNodes)
            : base()
        {
            WhichNodes = whichNodes;
            AgentsNeeded = 1;
        }
    }

    public class Node
    {
    }

}
