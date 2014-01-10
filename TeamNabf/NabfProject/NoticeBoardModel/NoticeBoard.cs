using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSLibrary.Data;
using System.Linq;
using NabfProject.AI;

namespace NabfProject.NoticeBoardModel
{
    public class NoticeBoard
    {
        private DictionaryList<JobType, Notice> _availableJobs = new DictionaryList<JobType, Notice>();
        private SortedList<int, Notice[]> _jobs = new SortedList<int, Notice[]>(new InvertedComparer<int>());
        private Dictionary<int, Notice> _idToNotice = new Dictionary<int, Notice>();
        private DictionaryList<NabfAgent, Notice> _agentToNotice = new DictionaryList<NabfAgent, Notice>();

        public SortedList<int, Notice[]> GetJobs()
        {
            SortedList<int, Notice[]> newjobs = new SortedList<int, Notice[]>(new InvertedComparer<int>());
            foreach (KeyValuePair<int, Notice[]> kvp in _jobs)
                newjobs.Add(kvp.Key, kvp.Value);
            return newjobs;
        }
        public void AddJob(Notice n)
        {
            if (_jobs.ContainsKey(n.HighestAverageDesirabilityForNotice))
            {
                List<Notice> l = _jobs[n.HighestAverageDesirabilityForNotice].ToList();
                l.Add(n);
                _jobs.Remove(n.HighestAverageDesirabilityForNotice);
                _jobs.Add(n.HighestAverageDesirabilityForNotice, l.ToArray());
            }
            else
                _jobs.Add(n.HighestAverageDesirabilityForNotice, new Notice[] { n });
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

        public enum JobType { Disrupt, Occupy, Attack, Repair }

        public event EventHandler<NoticeIsReadyToBeExecutedEventArgs> NoticeIsReadyToBeExecutedEvent;

        public NoticeBoard()
        {        
        }

        public bool AddNotice(Notice no)
        {
            if (AvailableJobsContainsChildType(no))
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
            if (!AvailableJobsContainsChildType(no))
                return false;

            foreach (NabfProject.AI.NabfAgent a in no.AgentsApplied)
                UnApplyToNotice(no, a);
            _idToNotice.Remove(no.Id);
            _availableJobs.Remove(NoticeToJobType(no), no);
            return true;
        }

        public bool UpdateNotice(int id, List<Node> whichNodes, int agentsNeeded)
        {
            Notice no;
            bool b = _idToNotice.TryGetValue(id, out no);

            if (b == false)
                return false;

            no.WhichNodes = whichNodes;
            no.AgentsNeeded = agentsNeeded;

            return true;
        }

        private JobType NoticeToJobType(Notice no)
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

        public void ApplyToNotice(Notice notice, int desirability, NabfAgent a)
        {
            foreach (Notice n in _availableJobs.Get(NoticeToJobType(notice)))
            {
                if (n.ChildTypeIsEqualTo(notice))
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
                if (n.ChildTypeIsEqualTo(notice))
                {
                    n.UnApply(a, this); //duplicate / false jobs are added here
                    _agentToNotice.Remove(a, n);
                    break;
                }
            }
        }

        private Notice PopFromJobsList()
        {
            if (_jobs.Count == 0)// || _jobs.First().Value.Count() == 0
                return null;
            Notice n = _jobs.First().Value.First();
            //Notice[] notices;
            //int i;
            if (_jobs.First().Value.Count() == 1)
                _jobs.RemoveAt(0);
            else
            {
                KeyValuePair<int, Notice[]> kv = _jobs.First();
                _jobs.Remove(kv.Key);
                _jobs.Add(kv.Key, kv.Value.Skip(1).ToArray());

                //notices = new Notice[_jobs.First().Value.Length - 1];
                //i = 0;
                //foreach(Notice no in notices)
                //{
                //    notices[i] = _jobs.First().Value[i + 1];
                //    i++;
                //}
                //KeyValuePair<int, Notice[]> kvp = new KeyValuePair<int, Notice[]>(_jobs.First().Key, notices);
                //_jobs.RemoveAt(0);
                //_jobs.Add(kvp.Key, kvp.Value);
            }
            return n;
        }
        
        private bool AvailableJobsContainsChildType(Notice no)
        {
            foreach (Notice n in _availableJobs.Get(NoticeToJobType(no)))
            {
                if (n.ChildTypeIsEqualTo(no))
                    return true;
            }
            return false;
        }

        public int FindTopDesiresForNotice(Notice n, out SortedList<int, NabfAgent> topDesires, out List<NabfAgent> agents)
        {
            //List<NabfAgent> agents;
            //SortedList<int, NabfAgent> topDesires;
            int desire = 0, lowestDesire = -(n.AgentsApplied.Count + 1);

            agents = new List<NabfAgent>();
            topDesires = new SortedList<int, NabfAgent>(new InvertedComparer<int>());
            for (int i = 0; i < n.AgentsNeeded; i++)
            {
                topDesires.Add(lowestDesire--, null);
            }
            desire = 0;
            lowestDesire = -1;

            foreach (NabfAgent a in n.AgentsApplied)
            {
                n.AgentsToDesirability.TryGetValue(a, out desire);
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

        private SortedList<int, Notice[]> PrepareSortedQueue()
        {
            DictionaryList<int, Notice> dl = new DictionaryList<int, Notice>();
            List<NabfAgent> agents;
            SortedList<int, NabfAgent> topDesires;
            int lowestDesire;
            //topDesires.OrderByDescending((kvp) => { return kvp.Key; });

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
            NoticeIsReadyToBeExecutedEventArgs args = new NoticeIsReadyToBeExecutedEventArgs();
            //List<NabfAgent> agents = new List<NabfAgent>();
            //Tuple<int, Notice[]> updatedJob;// = new DictionaryList<int, Notice>();
            //List<NabfAgent> agentsToAdd = new List<NabfAgent>();
            //SortedList<int, NabfAgent> topDesires = new SortedList<int,NabfAgent>();
            //int lowestDesire;
            //agents.AddRange(n.GetTopDesireAgents());
            args.Agents = n.GetTopDesireAgents();
            args.Notice = n;

            if (NoticeIsReadyToBeExecutedEvent != null)
                NoticeIsReadyToBeExecutedEvent(this, args);
            foreach (NabfAgent a in n.GetTopDesireAgents())
            { 
                foreach (Notice no in _agentToNotice[a])
                {
                    if (no.ChildTypeIsEqualTo(n))
                        continue;
                    UnApplyToNotice(no, a);
                }
            }
            #region legacy code
            /*foreach (NabfAgent a in agents)
            {//debug crash her. 
                foreach (Notice no in _agentToNotice[a])
                {
                    if (no.ChildTypeIsEqualTo(n))
                        continue;
                    UnApplyToNotice(no, a);
                    continue;
                    lowestDesire = FindTopDesiresForNotice(no, out topDesires, out agentsToAdd);
                    if (lowestDesire != -1)
                    {
                        if (_jobs.Keys.Contains(no.HighestAverageDesirabilityForNotice))
                        {
                            updatedJob = new Tuple<int, Notice[]>(no.HighestAverageDesirabilityForNotice, _jobs[no.HighestAverageDesirabilityForNotice]);
                            _jobs.Remove(no.HighestAverageDesirabilityForNotice);
                            foreach (Notice not in updatedJob.Item2)
                            {
                                if (not.ChildTypeIsEqualTo(no))
                                {
                                    not.HighestAverageDesirabilityForNotice = topDesires.Keys.Sum() / topDesires.Keys.Count;
                                    not.AddRangeToTopDesireAgents(agentsToAdd);
                                    if (_jobs.Keys.Contains(not.HighestAverageDesirabilityForNotice))
                                    {
                                        Notice[] nos = new Notice[_jobs[not.HighestAverageDesirabilityForNotice].Length + 1];
                                        for (int i = 0; i < nos.Length - 1; i++)
                                        {
                                            nos[i] = _jobs[not.HighestAverageDesirabilityForNotice][i];
                                        }
                                        nos[nos.Length - 1] = not;
                                        _jobs[not.HighestAverageDesirabilityForNotice] = nos;
                                    }
                                    else
                                    {
                                        _jobs.Add(not.HighestAverageDesirabilityForNotice, new Notice[] { not });
                                    }
                                }
                            }
                        }
                        else { } //handle if no desire exists yet

                        //n.HighestAverageDesirabilityForNotice = topDesires.Keys.Sum() / topDesires.Keys.Count;
                        //updatedJob = new Tuple<int, Notice[]>(topDesires.Keys.Sum() / topDesires.Keys.Count, agentsToAdd.ToArray());
                        //dl.Add(n.HighestAverageDesirabilityForNotice, n);
                        //n.TopDesireAgents = agentsToAdd;
                    }
                    else 
                    {                        
                        List<Notice> l = _jobs[no.HighestAverageDesirabilityForNotice].ToList();
                        l.Remove(no);
                        _jobs[no.HighestAverageDesirabilityForNotice] = l.ToArray();
                    }
                }
            }*/
            #endregion
            

            return true;
        }

        public void FindJobsForAgents()
        {
            SortedList<int, Notice[]> jobs = PrepareSortedQueue();
            _jobs = new SortedList<int, Notice[]>(new InvertedComparer<int>());
            for (int i = jobs.Count - 1; i > -1; i--)
            {
                _jobs.Add(jobs.Keys[i],jobs.Values[i]);
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

            //KeyValuePair<int, Notice[]> kvp;

            //for (int i = 0; i < _jobs.Keys.Count; i++)
            //{
            //    kvp = new KeyValuePair<int, Notice[]>(_jobs.Keys[i], _jobs.Values[i]);
            //    foreach (Notice n in kvp.Value)
            //    {
            //        if (n.TopDesireAgents.Count >= n.AgentsNeeded)
            //            RaiseEventForNotice(n);
            //    }
            //}

            //foreach (KeyValuePair<int, Notice[]> kvp in jobs.Reverse())
            //{
            //    foreach (Notice n in kvp.Value)
            //    {
            //        if (n.TopDesireAgents.Count >= n.AgentsNeeded)
            //            RaiseEventForNotice(n);
            //    }
            //}
        }
    }

    public class NoticeIsReadyToBeExecutedEventArgs : EventArgs
    {
        public Notice Notice { get; set; }
        public List<NabfAgent> Agents = new List<NabfAgent>();
    }
}
