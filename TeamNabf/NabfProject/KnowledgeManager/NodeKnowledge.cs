﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NabfProject.KnowledgeManager
{
    public class NodeKnowledge : Knowledge
    {
        public string Name { get; private set; }
        public int Value { get; private set;}

        public NodeKnowledge(string name, int value)
        {
            Name = name;
            Value = value;
        }

        bool IEquatable<Knowledge>.Equals(Knowledge other)
        {
            if (other == null)
                throw new ArgumentException("Input of Equals of " + this.GetType().Name + " is null");
            else if (!(other is Knowledge))
                throw new ArgumentException("Object : " + other.GetType().Name + " of Equals is not of type Knowledge");

            if (other.GetType() != this.GetType())
                return false;

            NodeKnowledge nk = (NodeKnowledge)other;
            
            return nk.Name == this.Name;
        }

        int IComparable<Knowledge>.CompareTo(Knowledge other)
        {
            if (other == null)
                throw new ArgumentException("Input of CompareTo of " + this.GetType().Name + " is null");
            else if (other is NodeKnowledge)
                if (((NodeKnowledge)other).Value < Value)
                    return -1;
                else if (((NodeKnowledge)other).Value > Value)
                    return 1;
                else
                    return 0;
            else
                throw new ArgumentException("Object : " + other.GetType().Name + " of CompareTo is not of type NodeKnowledge");
        }
    }
}
