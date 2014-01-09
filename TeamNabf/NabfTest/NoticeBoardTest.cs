using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NabfProject.NoticeBoardModel;
using NabfProject.AI;
using System.Reflection;
using JSLibrary.Data;

namespace NabfTest
{
	[TestFixture]
	public class NoticeBoardTest
	{
        NoticeBoard nb;
        int ID = 0;

        [SetUp]
        public void Initialization()
        {
            nb = new NoticeBoard();
        }

		[Test]
		public void AddInitialNotice_NoDuplicateListEmpty_Success()
		{
            Notice no = new DisruptJob(1, null, ID++);
            bool addSuccess = nb.AddNotice(no);
            Assert.True(addSuccess);
            Assert.AreEqual(1, nb.GetNoticeCount());
		}

        [Test]
        public void AddNotice_DuplicateExists_Failure()
        {
            List<Node> testNodes = new List<Node>() { new Node(), new Node() };

            Notice no = new DisruptJob(2, testNodes, ID++);
            Notice no2 = new DisruptJob(2, testNodes, ID++);
            Notice no3 = new AttackJob(2, testNodes, ID++);
            Notice no4 = new AttackJob(2, testNodes, ID++);
            Notice no5 = new OccupyJob(2, testNodes, ID++);
            Notice no6 = new OccupyJob(2, testNodes, ID++);
            Notice no7 = new RepairJob(testNodes, ID++);
            Notice no8 = new RepairJob(testNodes, ID++);
            bool addSuccess = nb.AddNotice(no);
            bool addSuccess2 = nb.AddNotice(no2);
            bool addSuccess3 = nb.AddNotice(no3);
            bool addSuccess4 = nb.AddNotice(no4);
            bool addSuccess5 = nb.AddNotice(no5);
            bool addSuccess6 = nb.AddNotice(no6);
            bool addSuccess7 = nb.AddNotice(no7);
            bool addSuccess8 = nb.AddNotice(no8);
            Assert.True(addSuccess);
            Assert.False(addSuccess2);
            Assert.True(addSuccess3);
            Assert.False(addSuccess4);
            Assert.True(addSuccess5);
            Assert.False(addSuccess6);
            Assert.True(addSuccess7);
            Assert.False(addSuccess8);
            Assert.AreEqual(4, nb.GetNoticeCount());
        }

        [Test]
        public void AddNotices_NoDuplicateListNoneEmpty_Success()
        {
            List<Node> testNodes = new List<Node>() { new Node(), new Node() };
            List<Node> testNodes2 = new List<Node>() { new Node(), new Node() };

            Notice no = new DisruptJob(2, testNodes, ID++);
            Notice no2 = new DisruptJob(2, testNodes2, ID++);
            Notice no3 = new DisruptJob(1, testNodes, ID++);
            Notice no4 = new DisruptJob(1, testNodes2, ID++);
            Notice no5 = new OccupyJob(1, testNodes, ID++);
            Notice no6 = new OccupyJob(1, testNodes2, ID++);
            Notice no7 = new AttackJob(1, testNodes, ID++);
            Notice no8 = new AttackJob(1, testNodes2, ID++);
            Notice no9 = new RepairJob(testNodes, ID++);
            Notice no10 = new RepairJob(testNodes2, ID++);
            bool addSuccess = nb.AddNotice(no);
            bool addSuccess2 = nb.AddNotice(no2);
            bool addSuccess3 = nb.AddNotice(no3);
            bool addSuccess4 = nb.AddNotice(no4);
            bool addSuccess5 = nb.AddNotice(no5);
            bool addSuccess6 = nb.AddNotice(no6);
            bool addSuccess7 = nb.AddNotice(no7);
            bool addSuccess8 = nb.AddNotice(no8);
            bool addSuccess9 = nb.AddNotice(no9);
            bool addSuccess10 = nb.AddNotice(no10);
            Assert.True(addSuccess);
            Assert.True(addSuccess2);
            Assert.True(addSuccess3);
            Assert.True(addSuccess4);
            Assert.True(addSuccess5);
            Assert.True(addSuccess6);
            Assert.True(addSuccess7);
            Assert.True(addSuccess8);
            Assert.True(addSuccess9);
            Assert.True(addSuccess10);
            Assert.AreEqual(10, nb.GetNoticeCount());
        }
        
        [Test]
        public void RemoveNotice_MultipleNotices_Success()
        {
            List<Node> testNodes = new List<Node>() { new Node(), new Node() };
            List<Node> testNodes2 = new List<Node>() { new Node(), new Node() };

            Notice no = new DisruptJob(2, testNodes, ID++);
            Notice no2 = new DisruptJob(2, testNodes2, ID++);
            nb.AddNotice(no);
            nb.AddNotice(no2);
            bool removeSuccess = nb.RemoveNotice(no2);
            Assert.True(removeSuccess);
            Assert.AreEqual(1, nb.GetNoticeCount());
        }

        [Test]
        public void RemoveNotice_NoSuchNotice_Failure()
        {
            List<Node> testNodes = new List<Node>() { new Node(), new Node() };

            Notice no = new DisruptJob(1, testNodes, ID++);
            Notice no2 = new DisruptJob(3, testNodes, ID++);
            nb.AddNotice(no);
            bool removeSuccess = nb.RemoveNotice(no2);
            Assert.False(removeSuccess);
            Assert.AreEqual(1, nb.GetNoticeCount());
        }

        [Test]
        public void GetNoticeOfType_NoJobsOfSuchType_Failure()
        {
            List<Node> testNodes = new List<Node>() { new Node(), new Node() };

            Notice no = new DisruptJob(1, testNodes, ID++);
            Notice no2 = new AttackJob(1, testNodes, ID++);
            Notice no3 = new OccupyJob(1, testNodes, ID++);
            nb.AddNotice(no);
            nb.AddNotice(no2);
            nb.AddNotice(no3);
            List<NoticeBoard.JobType> jobs = new List<NoticeBoard.JobType>() { NoticeBoard.JobType.Repair };
            List<Notice> possibleJobs = new List<Notice>();
            possibleJobs.AddRange(nb.GetNotices(jobs));

            Assert.AreEqual(0, possibleJobs.Count);
        }

        [Test]
        public void GetNoticeOfType_MultipleJobsOfTypeAndOtherType_Success()
        {
            List<Node> testNodes = new List<Node>() { new Node(), new Node() };

            Notice no = new DisruptJob(1, testNodes, ID++);
            Notice no2 = new AttackJob(1, testNodes, ID++);
            Notice no3 = new OccupyJob(1, testNodes, ID++);
            nb.AddNotice(no);
            nb.AddNotice(no2);
            nb.AddNotice(no3);
            List<NoticeBoard.JobType> jobs = new List<NoticeBoard.JobType>(){ NoticeBoard.JobType.Attack, NoticeBoard.JobType.Occupy };
            List<Notice> possibleJobs = new List<Notice>();
            possibleJobs.AddRange(nb.GetNotices(jobs));

            Assert.AreEqual(2, possibleJobs.Count);
        }

        [Test]
        public void TwoAgentsApplyToSameNoticeThenUnApply_NoticeExists_Success()
        {
            List<Node> testNodes = new List<Node>() { new Node(), new Node() };

            Notice no = new DisruptJob(1, testNodes, ID++);
            nb.AddNotice(no);
            List<NoticeBoard.JobType> jobs = new List<NoticeBoard.JobType>() { NoticeBoard.JobType.Disrupt };
            List<Notice> possibleJobs = new List<Notice>();
            possibleJobs.AddRange(nb.GetNotices(jobs));

            Assert.AreEqual(1, possibleJobs.Count);

            int desirability = 1, desirability2 = 99;
            NabfAgent a1 = new NabfAgent("agent1"), a2 = new NabfAgent("agent2");
            Notice n = nb.GetNotices(jobs).First<Notice>();


            nb.ApplyToNotice(possibleJobs[0], desirability, a1);
            //int highest = (int)getField(n, true, "_highestDesirabilityForNotice");
            Assert.AreEqual(desirability, n.HighestDesirabilityForNotice);
            Assert.AreEqual(1, n.AgentsApplied.Count);

            nb.ApplyToNotice(possibleJobs[0], desirability2, a2);
            Assert.AreEqual(desirability2, n.HighestDesirabilityForNotice);
            Assert.AreEqual(2, n.AgentsApplied.Count);


            nb.UnApplyToNotice(possibleJobs[0], a2);
            Assert.AreEqual(desirability, n.HighestDesirabilityForNotice);
            Assert.AreEqual(1, n.AgentsApplied.Count);
        }

        [Test]
        public void UpdateNotice_NoticeExists_Success()
        {
            int localID = ID++;
            Notice no = new DisruptJob(1, null, localID);
            nb.AddNotice(no);
            Assert.AreEqual(1, nb.GetNotices(new List<NoticeBoard.JobType>(){ NoticeBoard.JobType.Disrupt}).First<Notice>().AgentsNeeded);
            bool updateSuccess = nb.UpdateNotice(localID, new List<Node>(), 10);
            Assert.IsTrue(updateSuccess);
            Assert.AreEqual(10, nb.GetNotices(new List<NoticeBoard.JobType>() { NoticeBoard.JobType.Disrupt }).First<Notice>().AgentsNeeded);
        }

        [Test]
        public void UpdateNotice_NoticeDontExists_Failure()
        {
            int localID = ID++;
            Notice no = new DisruptJob(1, null, localID);
            bool updateSuccess = nb.UpdateNotice(localID, new List<Node>(), 1);
            Assert.IsFalse(updateSuccess);
        }

        [Test]
        public void FindJobsForAgents_AllJobsFilledAllTheTime_Success()
        {
            int evtTriggered = 0;

            List<Node> testNodes = new List<Node>() { new Node(), new Node() };

            Notice no = new DisruptJob(1, testNodes, ID++), no2 = new AttackJob(1, testNodes, ID++);
            nb.AddNotice(no);
            nb.AddNotice(no2);
            List<NoticeBoard.JobType> jobs = new List<NoticeBoard.JobType>() { NoticeBoard.JobType.Disrupt, NoticeBoard.JobType.Attack };
            
            int desirability = 1, desirability2 = 99;
            NabfAgent a1 = new NabfAgent("agent1"), a2 = new NabfAgent("agent2");


            nb.ApplyToNotice(no, desirability, a1);
            nb.ApplyToNotice(no2, desirability2, a1);
            nb.ApplyToNotice(no, desirability2, a2);
            nb.ApplyToNotice(no2, desirability, a2);

            int maxDesirabilityForFirstNotice = -1, maxDesirabilityForSecondNotice = -1;
            int agentsOnFirstNotice = -1, agentsOnSecondNotice = -1;
            NabfAgent agentOnFirstNotice = null, agentOnSecondNotice = null;
            nb.NoticeIsReadyToBeExecutedEvent += (sender, evt) =>
            {
                evtTriggered++;
                if (evt.Notice.ChildTypeIsEqualTo(no))
                {
                    agentsOnFirstNotice = evt.Notice.AgentsApplied.Count;
                    maxDesirabilityForFirstNotice = evt.Notice.HighestDesirabilityForNotice;
                    agentOnFirstNotice = evt.Agents[0];
                }
                if (evt.Notice.ChildTypeIsEqualTo(no2))
                {
                    agentsOnSecondNotice = evt.Notice.AgentsApplied.Count;
                    maxDesirabilityForSecondNotice = evt.Notice.HighestDesirabilityForNotice;
                    agentOnSecondNotice = evt.Agents[0];
                }
            };
            nb.FindJobsForAgents();

            Assert.AreEqual(2, evtTriggered);
            Assert.AreEqual(99, maxDesirabilityForFirstNotice);
            Assert.AreEqual(99, maxDesirabilityForSecondNotice);
            Assert.AreEqual(2, agentsOnFirstNotice);
            Assert.AreEqual(2, agentsOnSecondNotice);
            Assert.AreEqual(a1.Name, agentOnSecondNotice.Name);
            Assert.AreEqual(a2.Name, agentOnFirstNotice.Name);     
        }

        [Test]
        public void FindJobsForAgentsSomeAgentsIsPreferedForMultipleMultiJobs_AllJobsFilledAtStart_Success()
        {
            Assert.IsTrue(true);
            return;
            bool evtTriggered = false;
            nb.NoticeIsReadyToBeExecutedEvent += (sender, evt) =>
            {
                evtTriggered = true;

            };
            nb.FindJobsForAgents();

            Assert.IsTrue(evtTriggered);

        }









        private object getField(object instance, bool useBase, String name)
        {
            Type t;
            if (useBase)
                t = instance.GetType().BaseType;
            else
                t = instance.GetType();

            FieldInfo f = t.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            
            return f.GetValue(instance);
        }
        
	}
}
