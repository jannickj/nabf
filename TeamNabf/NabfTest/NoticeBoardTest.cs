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

            Notice no = new DisruptJob(1, testNodes, ID++), no2 = new AttackJob(1, testNodes, ID++), no3 = new OccupyJob(2, testNodes, ID++);
            nb.AddNotice(no);
            nb.AddNotice(no2);
            nb.AddNotice(no3);
            //List<NoticeBoard.JobType> jobs = new List<NoticeBoard.JobType>() { NoticeBoard.JobType.Disrupt, NoticeBoard.JobType.Attack };
            //List<NoticeBoard.JobType> jobs2 = new List<NoticeBoard.JobType>() { NoticeBoard.JobType.Disrupt, NoticeBoard.JobType.Attack, NoticeBoard.JobType.Occupy };
            
            //int desirability = 1, desirability2 = 100, desirability3 = 50;
            NabfAgent a1 = new NabfAgent("agent1"), a2 = new NabfAgent("agent2"), a3 = new NabfAgent("agent3"), a4 = new NabfAgent("agent4");


            nb.ApplyToNotice(no, 1, a1);
            nb.ApplyToNotice(no2, 50, a1);
            nb.ApplyToNotice(no, 50, a2);
            nb.ApplyToNotice(no2, 1, a2);

            nb.ApplyToNotice(no, 1, a3);

            nb.ApplyToNotice(no3, 50, a3);
            nb.ApplyToNotice(no3, 100, a4);

            int maxDesirabilityForFirstNotice = -1, maxDesirabilityForSecondNotice = -1, maxDesirabilityForThirdNotice = -1;
            int agentsAppliedToFirstNotice = -1, agentsAppliedToSecondNotice = -1, agentsAppliedToThirdNotice = -1;
            NabfAgent agentOnFirstNotice1 = null, agentOnFirstNotice2 = null, agentOnSecondNotice = null, agentOnThirdNotice = null;
            int i = 0;
            Notice firstNotice = null, secondNotice = null, thirdNotice = null;
            nb.NoticeIsReadyToBeExecutedEvent += (sender, evt) =>
            {
                evtTriggered++;
                i++;
                if (i == 1)
                {
                    firstNotice = evt.Notice;
                    agentsAppliedToFirstNotice = evt.Notice.AgentsApplied.Count;
                    maxDesirabilityForFirstNotice = evt.Notice.HighestAverageDesirabilityForNotice;
                    agentOnFirstNotice1 = evt.Agents[0];
                    agentOnFirstNotice2 = evt.Agents[1];
                }
                if (i == 2)
                {
                    secondNotice = evt.Notice;
                    agentsAppliedToSecondNotice = evt.Notice.AgentsApplied.Count;
                    maxDesirabilityForSecondNotice = evt.Notice.HighestAverageDesirabilityForNotice;
                    agentOnSecondNotice = evt.Agents[0];
                }
                if (i == 3)
                {
                    thirdNotice = evt.Notice;
                    agentsAppliedToThirdNotice = evt.Notice.AgentsApplied.Count;
                    maxDesirabilityForThirdNotice = evt.Notice.HighestAverageDesirabilityForNotice;
                    agentOnThirdNotice = evt.Agents[0];
                }
            };
            nb.FindJobsForAgents();

            Assert.AreEqual(3, evtTriggered);
            Assert.IsTrue(firstNotice.ChildTypeIsEqualTo(no3));
            Assert.IsTrue(secondNotice.ChildTypeIsEqualTo(no2) || secondNotice.ChildTypeIsEqualTo(no));
            Assert.IsTrue(thirdNotice.ChildTypeIsEqualTo(no2) || thirdNotice.ChildTypeIsEqualTo(no));
            Assert.AreEqual(75, maxDesirabilityForFirstNotice);
            Assert.AreEqual(50, maxDesirabilityForSecondNotice);
            Assert.AreEqual(50, maxDesirabilityForThirdNotice);
            //Assert.AreEqual(2, agentsAppliedToFirstNotice);
            //Assert.AreEqual(3, agentsAppliedToSecondNotice);
            //Assert.AreEqual(2, agentsAppliedToThirdNotice);
            Assert.AreEqual(a3.Name, agentOnFirstNotice1.Name);
            Assert.AreEqual(a4.Name, agentOnFirstNotice2.Name);
            Assert.AreEqual(a2.Name, agentOnSecondNotice.Name);
            Assert.AreEqual(a1.Name, agentOnThirdNotice.Name);    
        }

        [Test]
        public void FindJobsForAgentsSomeAgentsIsPreferedForMultipleMultiJobs_AllJobsFilledAtStart_Success()
        {
            #region setup
            List<Node> nodes = new List<Node>() { new Node(), new Node(), new Node() };

            NabfAgent agent1 = new NabfAgent("a1"), agent2 = new NabfAgent("a2"), agent3 = new NabfAgent("a3"), 
                agent4 = new NabfAgent("a4"), agent5 = new NabfAgent("a5"), agent6 = new NabfAgent("a6");

            Notice notice1 = new OccupyJob(2, nodes, ID++), notice2 = new DisruptJob(2, nodes, ID++), notice3 = new AttackJob(2, nodes, ID++), 
                notice4 = new RepairJob(nodes, ID++);

            nb.AddNotice(notice1); nb.AddNotice(notice2); nb.AddNotice(notice3); nb.AddNotice(notice4);
            #endregion

            #region agent desirability
            nb.ApplyToNotice(notice1, 110, agent1);
            nb.ApplyToNotice(notice2, 10, agent1);
            nb.ApplyToNotice(notice3, 11, agent1);
            nb.ApplyToNotice(notice4, 1111, agent1);

            nb.ApplyToNotice(notice1, 120, agent2);
            nb.ApplyToNotice(notice2, 20, agent2);
            nb.ApplyToNotice(notice3, 22, agent2);
            nb.ApplyToNotice(notice4, 2222, agent2);

            nb.ApplyToNotice(notice1, 130, agent3);
            nb.ApplyToNotice(notice2, 30, agent3);
            nb.ApplyToNotice(notice3, 33, agent3);

            nb.ApplyToNotice(notice1, 140, agent4);
            nb.ApplyToNotice(notice2, 40, agent4);
            nb.ApplyToNotice(notice3, 44, agent4);

            nb.ApplyToNotice(notice1, 150, agent5);
            nb.ApplyToNotice(notice2, 50, agent5);
            nb.ApplyToNotice(notice3, 55, agent5);

            nb.ApplyToNotice(notice1, 99999999, agent6);
            nb.ApplyToNotice(notice2, 99999999, agent6);
            nb.ApplyToNotice(notice3, 99999999, agent6);
            #endregion

            int evtTriggered = 0;
            List<Notice> notices = new List<Notice>();
            List<List<NabfAgent>> listOfListOfAgents = new List<List<NabfAgent>>();
            List<int> averageDesires = new List<int>();
            nb.NoticeIsReadyToBeExecutedEvent += (sender, evt) =>
            {
                notices.Add(evt.Notice);
                listOfListOfAgents.Add(evt.Agents);
                averageDesires.Add(evt.Notice.HighestAverageDesirabilityForNotice);

                evtTriggered++;
            };
            nb.FindJobsForAgents();

            Assert.AreEqual(3, evtTriggered);

            Assert.AreEqual(notice1, notices[0]);
            //Assert.AreEqual(0, notices);
            Assert.AreEqual(notice4, notices[1]);
            Assert.AreEqual(notice3, notices[2]);

            Assert.IsTrue(listOfListOfAgents[0][0].Name == agent6.Name || listOfListOfAgents[0][0].Name == agent5.Name);
            Assert.IsTrue(listOfListOfAgents[0][1].Name == agent6.Name || listOfListOfAgents[0][1].Name == agent5.Name);
            Assert.AreEqual(listOfListOfAgents[1][0].Name, agent2.Name);
            Assert.IsTrue(listOfListOfAgents[2][0].Name == agent4.Name || listOfListOfAgents[2][0].Name == agent3.Name);
            Assert.IsTrue(listOfListOfAgents[2][1].Name == agent4.Name || listOfListOfAgents[2][1].Name == agent3.Name);

            Assert.AreEqual((99999999 + 150) / 2, averageDesires[0]);
            Assert.AreEqual(2222, averageDesires[1]);
            Assert.AreEqual((44 + 33) / 2, averageDesires[2]);

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
