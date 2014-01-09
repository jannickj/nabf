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
        private DictionaryList<JobType, Notice> AvailableJobs = new DictionaryList<JobType, Notice>();

        public enum JobType { Disrupt, Occupy, Attack, Repair }

        public event EventHandler<NoticeIsReadyToBeExecutedEventArgs> NoticeIsReadyToBeExecutedEvent;

        public NoticeBoard()
        {        
        }

        public bool AddNotice(Notice no)
        {
            if (AvailableJobsContainsChildType(no))
                return false;
            AvailableJobs.Add(NoticeToJobType(no), no);
            return true;
        }

        private bool AvailableJobsContainsChildType(Notice no)
        {
            foreach (Notice n in AvailableJobs.Get(NoticeToJobType(no)))
            {
                if (n.ChildTypeIsEqualTo(no))
                    return true;
            }
            return false;
        }

        public int GetNoticeCount()
        {
            return AvailableJobs.TotalCount;
        }

        public int GetNoticeCount(Notice ofType)
        {
            return AvailableJobs.Get(NoticeToJobType(ofType)).Count;
        }

        public int GetNoticeCount(JobType ofType)
        {
            return AvailableJobs.Get(ofType).Count;
        }

        public bool RemoveNotice(Notice no)
        {
            if (!AvailableJobsContainsChildType(no))
                return false;

            foreach (NabfProject.AI.NabfAgent a in no.AgentsApplied)
                UnApplyToNotice(no, a);
            AvailableJobs.Remove(NoticeToJobType(no), no);
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
                result.AddRange(AvailableJobs.Get(jb));
            }
            return result;
        }

        public void ApplyToNotice(Notice notice, int desirability, NabfAgent a)
        {
            foreach (Notice n in AvailableJobs.Get(NoticeToJobType(notice)))
            {
                if (n.ChildTypeIsEqualTo(notice))
                {
                    n.Apply(desirability, a);
                    break;
                }
            }
        }
        public void UnApplyToNotice(Notice notice, NabfAgent a)
        {
            foreach (Notice n in AvailableJobs.Get(NoticeToJobType(notice)))
            {
                if (n.ChildTypeIsEqualTo(notice))
                {
                    n.UnApply(a);
                    break;
                }
            }
        }

        //public Notice PopFromJobsList(SortedList<int, Notice> jobs)
        //{
        //    Notice n = jobs.First<KeyValuePair<int, Notice>>().Value;
        //    jobs.RemoveAt(0);
        //    return n;
        //}

        public SortedList<int, Notice[]> PrepareSortedQueue()
        {
            //calculate highest desirability based on the average desirability of the top X agents, where X is AgentsNeeded
            DictionaryList<int, Notice> dl = new DictionaryList<int, Notice>();
            foreach (Notice no in AvailableJobs.SelectMany(kvp => kvp.Value))
                dl.Add(no.HighestDesirabilityForNotice, no);

            SortedList<int, Notice[]> jobs = new SortedList<int, Notice[]>();

            foreach (KeyValuePair<int, Notice[]> kvp in dl)
                jobs.Add(kvp.Key, kvp.Value);

            return jobs;
        }

        private bool RaiseEventForNotice(Notice n) 
        {
            //handle if not enough have applied (or have been removed first)
            List<NabfAgent> agents = new List<NabfAgent>();
            NoticeIsReadyToBeExecutedEventArgs args = new NoticeIsReadyToBeExecutedEventArgs();
            SortedList<int, NabfAgent> topDesires = new SortedList<int, NabfAgent>(new InvertedComparer<int>());
            //topDesires.OrderByDescending((kvp) => { return kvp.Key; });
            for(int i = 0; i < n.AgentsNeeded; i++)
            {
                topDesires.Add(-1, null);
            }
            int desire = 0, lowestDesire = -1;
            bool b;
            
            foreach(NabfAgent a in n.AgentsApplied)
            {
                b = n.AgentsToDesirability.TryGetValue(a, out desire);
                if (desire > lowestDesire)
                {
                    topDesires.Add(desire, a);
                    agents.Add(a);
                    agents.Remove(topDesires.Last().Value);
                    topDesires.RemoveAt(n.AgentsNeeded);
                    lowestDesire = topDesires.Keys[n.AgentsNeeded - 1];
                }
            }
            if (lowestDesire == -1)
                return false;
            args.Agents = agents;
            args.Notice = n;
            
            if(NoticeIsReadyToBeExecutedEvent != null)
                NoticeIsReadyToBeExecutedEvent(this, args);

            return true;
        }

        public void FindJobsForAgents()
        {
            SortedList<int, Notice[]> jobs = PrepareSortedQueue();

            foreach (KeyValuePair<int, Notice[]> kvp in jobs)
            {                
                foreach(Notice n in kvp.Value)
                    RaiseEventForNotice(n);
            }
        }
    }

    public class NoticeIsReadyToBeExecutedEventArgs : EventArgs
    {
        public Notice Notice { get; set; }
        public List<NabfAgent> Agents = new List<NabfAgent>();
    }
}
