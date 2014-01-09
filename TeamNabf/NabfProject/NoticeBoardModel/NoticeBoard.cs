using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSLibrary.Data;
using System.Linq;

namespace NabfProject.NoticeBoardModel
{
    public class NoticeBoard
    {
        private SortedList<int, Notice> Jobs { get; set; }
        private DictionaryList<JobType, Notice> AvailableJobs = new DictionaryList<JobType, Notice>();

        public enum JobType { Disrupt, Occupy, Attack, Repair }

        public NoticeBoard()
        {
            Jobs = new SortedList<int, Notice>();            
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

        public void ApplyToNotice(Notice notice, int desirability, AI.NabfAgent a)
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
        public void UnApplyToNotice(Notice notice, AI.NabfAgent a)
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

        public Notice PopFromJobsList()
        {
            Notice n = Jobs.First<KeyValuePair<int, Notice>>().Value;
            Jobs.RemoveAt(0);
            return n;
        }

        //public bool PrepareSortedQueue() 
        //{
        //    foreach(KeyValuePair<JobType, Notice[]> kvp in AvailableJobs)
        //    {
        //        Jobs.Add();                
        //    }
            
        //    return true; 
        //}
    }
}
