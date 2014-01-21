using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSLibrary.Data;
using NabfProject.AI;
using NabfProject.Events;
using NabfProject.KnowledgeManagerModel;

namespace NabfProject.NoticeBoardModel
{
    public class NoticeBoard
    {
        private DictionaryList<JobType, Notice> _availableJobs = new DictionaryList<JobType, Notice>();
        private SortedList<int, Notice[]> _jobs = new SortedList<int, Notice[]>(new InvertedComparer<int>());
        private Dictionary<Int64, Notice> _idToNotice = new Dictionary<Int64, Notice>();
        private DictionaryList<NabfAgent, Notice> _agentToNotice = new DictionaryList<NabfAgent, Notice>();
        private Int64 _freeID = 0;
        private HashSet<NabfAgent> _sharingList = new HashSet<NabfAgent>();

        public enum JobType { Disrupt, Occupy, Attack, Repair }
        
        public NoticeBoard()
        {
        }

        public bool Subscribe(NabfAgent agent)
        {
            if (_sharingList.Contains(agent))
                return false;
            _sharingList.Add(agent);
            return true;
        }
        public bool Unsubscribe(NabfAgent agent)
        {
            return _sharingList.Remove(agent);
        }

        public SortedList<int, Notice[]> GetJobs()
        {
            SortedList<int, Notice[]> newjobs = new SortedList<int, Notice[]>(new InvertedComparer<int>());
            foreach (KeyValuePair<int, Notice[]> kvp in _jobs)
                newjobs.Add(kvp.Key, kvp.Value);
            return newjobs;
        }
        public bool AddJob(Notice n)
        {
            if (n.HighestAverageDesirabilityForNotice == -1)
                return false;
            if (_jobs.ContainsKey(n.HighestAverageDesirabilityForNotice))
            {
                List<Notice> l = _jobs[n.HighestAverageDesirabilityForNotice].ToList();
                l.Add(n);
                _jobs.Remove(n.HighestAverageDesirabilityForNotice);
                _jobs.Add(n.HighestAverageDesirabilityForNotice, l.ToArray());
            }
            else
                _jobs.Add(n.HighestAverageDesirabilityForNotice, new Notice[] { n });

            return true;
        }
        public bool RemoveJob(Notice n)
        {
            if (_jobs.ContainsKey(n.HighestAverageDesirabilityForNotice))
            {
                List<Notice> l = _jobs[n.HighestAverageDesirabilityForNotice].ToList();
                l.Remove(n);
                bool b = _jobs.Remove(n.HighestAverageDesirabilityForNotice);
                if (l.Count == 0)
                    return b;
                if (b)
                {
                    _jobs.Add(n.HighestAverageDesirabilityForNotice, l.ToArray());
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        private void ResetNoticeBoard()
        {
            _availableJobs.Clear();
            _jobs.Clear();
            _idToNotice.Clear();
            _agentToNotice.Clear();
            _freeID = 0;
        }

        public bool CreateAndAddNotice(JobType type, int agentsNeeded, List<NodeKnowledge> whichNodes, int value, out Notice notice)
        {
            Notice n = null;
            Int64 id = _freeID;
            _freeID++;
            switch (type)
            {
                case JobType.Attack:
                    n = new AttackJob(agentsNeeded, whichNodes, value, id);
                    break;
                case JobType.Disrupt:
                    n = new DisruptJob(agentsNeeded, whichNodes, value, id);
                    break;
                case JobType.Occupy:
                    n = new OccupyJob(agentsNeeded, whichNodes, value, id);
                    break;
                case JobType.Repair:
                    n = new RepairJob(whichNodes, value, id);
                    break;
            }
            if (n == null)
                throw new ArgumentException("Input to CreateNotice, JoType : " + type.GetType().Name + " was not of appropriate type. It's type was: " + type.GetType());

            bool b = AddNotice(n);
            notice = n;

            foreach (NabfAgent a in _sharingList)
                a.Raise(new NewNoticeEvent(n));

            return b;
        }

        private bool AddNotice(Notice no)
        {
            if (AvailableJobsContainsContentEqual(no))
                return false;
            _idToNotice.Add(no.Id, no);
            _availableJobs.Add(NoticeToJobType(no), no);
            return true;
        }

        public int GetNoticeCount()
        {
            return _availableJobs.TotalCount;
        }

        public int GetNoticeCount(Notice ofType)
        {
            return _availableJobs.Get(NoticeToJobType(ofType)).Count;
        }

        public int GetNoticeCount(JobType ofType)
        {
            return _availableJobs.Get(ofType).Count;
        }

        public bool RemoveNotice(Notice no)
        {
            if (!AvailableJobsContainsContentEqual(no))
                return false;

            foreach (NabfProject.AI.NabfAgent a in no.GetAgentsApplied())
                UnApplyToNotice(no, a);
            _idToNotice.Remove(no.Id);
            _availableJobs.Remove(NoticeToJobType(no), no);

            foreach (NabfAgent a in _sharingList)
                a.Raise(new NoticeRemovedEvent(no));

            return true;
        }

        public bool UpdateNotice(Int64 id, List<NodeKnowledge> whichNodes, int agentsNeeded, int value)
        {
            Notice no;
            bool b = _idToNotice.TryGetValue(id, out no);

            if (b == false)
                return false;

            no.UpdateNotice(whichNodes, agentsNeeded, value);

            foreach (NabfAgent a in _sharingList)
                a.Raise(new NoticeUpdatedEvent(id, no));

            return true;
        }

        public JobType NoticeToJobType(Notice no)
        {
            if (no == null)
                throw new ArgumentNullException("Input to method NoticeToType was null.");


            if (no is DisruptJob)
                return JobType.Disrupt;
            else if (no is AttackJob)
                return JobType.Attack;
            else if (no is OccupyJob)
                return JobType.Occupy;
            else if (no is RepairJob)
                return JobType.Repair;
            else
                throw new ArgumentException("Input to NoticeToJobtype, object : " + no.GetType().Name + " was not of appropriate type. It's type was: " + no.GetType());
        }

        public ICollection<Notice> GetNotices(List<JobType> jobs)
        {
            List<Notice> result = new List<Notice>();
            foreach(JobType jb in jobs)
            {
                result.AddRange(_availableJobs.Get(jb));
            }
            return result;
        }

        public ICollection<Notice> GetNotices()
        {
            List<JobType> jobs = new List<JobType>() { JobType.Attack, JobType.Disrupt, JobType.Occupy, JobType.Repair };
            List<Notice> result = new List<Notice>();
            foreach (JobType jb in jobs)
            {
                result.AddRange(_availableJobs.Get(jb));
            }
            return result;
        }

        public ICollection<Notice> GetNotices(NabfAgent agent)
        {
            throw new NotImplementedException();
        }

        public void ApplyToNotice(Notice notice, int desirability, NabfAgent a)
        {
            foreach (Notice n in _availableJobs.Get(NoticeToJobType(notice)))
            {
                if (n.ContentIsEqualTo(notice))
                {
                    n.Apply(desirability, a);
                    _agentToNotice.Add(a, n);
                    break;
                }
            }
        }
        public void UnApplyToNotice(Notice notice, NabfAgent a)
        {
            foreach (Notice n in _availableJobs.Get(NoticeToJobType(notice)))
            {
                if (n.ContentIsEqualTo(notice))
                {
                    n.UnApply(a, this);
                    _agentToNotice.Remove(a, n);
                    break;
                }
            }
        }

        public void UnApplyFromAll(NabfAgent a)
        {
            foreach (Notice n in _agentToNotice[a])
                UnApplyToNotice(n, a);
        }

        public int FindTopDesiresForNotice(Notice n, out SortedList<int, NabfAgent> topDesires, out List<NabfAgent> agents)
        {
            int desire = 0, lowestDesire = -(n.GetAgentsApplied().Count + 1);

            agents = new List<NabfAgent>();
            topDesires = new SortedList<int, NabfAgent>(new InvertedComparer<int>());
            for (int i = 0; i < n.AgentsNeeded; i++)
            {
                topDesires.Add(lowestDesire--, null);
            }
            desire = 0;
            lowestDesire = -1;

            foreach (NabfAgent a in n.GetAgentsApplied())
            {
                n.TryGetValueAgentToDesirabilityMap(a, out desire);
                if (desire > lowestDesire)
                {
                    topDesires.Add(desire, a);
                    agents.Add(a);
                    agents.Remove(topDesires.Last().Value);
                    topDesires.RemoveAt(n.AgentsNeeded);
                    lowestDesire = topDesires.Keys[n.AgentsNeeded - 1];
                }
            }

            return lowestDesire;
        }

        public void FindJobsForAgents()
        {
            SortedList<int, Notice[]> jobs = PrepareSortedQueue();
            _jobs = new SortedList<int, Notice[]>(new InvertedComparer<int>());
            for (int i = jobs.Count - 1; i > -1; i--)
            {
                _jobs.Add(jobs.Keys[i], jobs.Values[i]);
            }

            Notice notice;
            bool QueueNotEmpty = true;
            while (QueueNotEmpty)
            {
                notice = PopFromJobsList();
                QueueNotEmpty = (notice != null);
                if (notice != null && notice.AgentsNeeded <= notice.GetTopDesireAgents().Count)
                    RaiseEventForNotice(notice);
            }
        }

        private Notice PopFromJobsList()
        {
            if (_jobs.Count == 0)
                return null;
            Notice n = _jobs.First().Value.First();
            if (_jobs.First().Value.Count() == 1)
                _jobs.RemoveAt(0);
            else
            {
                KeyValuePair<int, Notice[]> kv = _jobs.First();
                _jobs.Remove(kv.Key);
                _jobs.Add(kv.Key, kv.Value.Skip(1).ToArray());
            }
            return n;
        }
        
        private bool AvailableJobsContainsContentEqual(Notice no)
        {
            foreach (Notice n in _availableJobs.Get(NoticeToJobType(no)))
            {
                if (n.ContentIsEqualTo(no))
                    return true;
            }
            return false;
        }

        private SortedList<int, Notice[]> PrepareSortedQueue()
        {
            DictionaryList<int, Notice> dl = new DictionaryList<int, Notice>();
            List<NabfAgent> agents;
            SortedList<int, NabfAgent> topDesires;
            int lowestDesire;

            foreach (Notice n in _availableJobs.SelectMany(kvp => kvp.Value))
            {
                lowestDesire = FindTopDesiresForNotice(n, out topDesires, out agents);
                if (lowestDesire != -1)
                {
                    n.HighestAverageDesirabilityForNotice = topDesires.Keys.Sum() / topDesires.Keys.Count;
                    dl.Add(n.HighestAverageDesirabilityForNotice, n);
                    n.AddRangeToTopDesireAgents(agents);
                }
            }
            SortedList<int, Notice[]> jobs = new SortedList<int, Notice[]>(new InvertedComparer<int>());

            foreach (KeyValuePair<int, Notice[]> kvp in dl)
                jobs.Add(kvp.Key, kvp.Value);

            return jobs;
        }

        private bool RaiseEventForNotice(Notice n) 
        {
            //NoticeIsReadyToBeExecutedEventArgs args = new NoticeIsReadyToBeExecutedEventArgs();
            //args.Agents = n.GetTopDesireAgents();
            //args.Notice = n;

            //if (NoticeIsReadyToBeExecutedEvent != null)
            //    NoticeIsReadyToBeExecutedEvent(this, args);
            foreach (NabfAgent a in n.GetTopDesireAgents())
            {
                a.Raise(new ReceivedJobEvent(n));
                foreach (Notice no in _agentToNotice[a])
                {
                    if (no.ContentIsEqualTo(n))
                        continue;
                    UnApplyToNotice(no, a);
                }
            }
            return true;
        }
    }

    
}
