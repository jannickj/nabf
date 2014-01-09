using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NabfProject.NoticeBoardModel;

namespace NabfTest
{
	[TestFixture]
	public class NoticeBoardTest
	{
        NoticeBoard nb;

        [SetUp]
        public void Initialization()
        {
            nb = new NoticeBoard();
        }

		[Test]
		public void AddInitialNotice_NoDuplicateListEmpty_Success()
		{
            Notice no = new DisruptJob(1, null);
            bool addSuccess = nb.AddNotice(no);
            Assert.True(addSuccess);
            Assert.AreEqual(1, nb.GetNoticeCount());
		}

        [Test]
        public void AddNotice_DuplicateExists_Failure()
        {
            List<Node> testNodes = new List<Node>() { new Node(), new Node() };

            Notice no = new DisruptJob(2, testNodes);
            Notice no2 = new DisruptJob(2, testNodes);
            Notice no3 = new AttackJob(2, testNodes);
            Notice no4 = new AttackJob(2, testNodes);
            Notice no5 = new OccupyJob(2, testNodes);
            Notice no6 = new OccupyJob(2, testNodes);
            Notice no7 = new RepairJob(testNodes);
            Notice no8 = new RepairJob(testNodes);
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

            Notice no = new DisruptJob(2, testNodes);
            Notice no2 = new DisruptJob(2, testNodes2);
            Notice no3 = new DisruptJob(1, testNodes);
            Notice no4 = new DisruptJob(1, testNodes2);
            Notice no5 = new OccupyJob(1, testNodes);
            Notice no6 = new OccupyJob(1, testNodes2);
            Notice no7 = new AttackJob(1, testNodes);
            Notice no8 = new AttackJob(1, testNodes2);
            Notice no9 = new RepairJob(testNodes);
            Notice no10 = new RepairJob(testNodes2);
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

            Notice no = new DisruptJob(2, testNodes);
            Notice no2 = new DisruptJob(2, testNodes2);
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

            Notice no = new DisruptJob(1, testNodes);
            Notice no2 = new DisruptJob(3, testNodes);
            nb.AddNotice(no);
            bool removeSuccess = nb.RemoveNotice(no2);
            Assert.False(removeSuccess);
            Assert.AreEqual(1, nb.GetNoticeCount());
        }

        [Test]
        public void GetNoticeOfType_NoTypes_Failure()
        {
        }

        //[Test]
        //public void GetNoticeOfType_NoTypes_Failure()
        //{
        //}
        //[Test]
        //public void RemoveNotice_NoSuchNotice_Failure()
        //{
        //}
        
        //remove og add af occupy jobs skal have testing for delte områder (se .txt fil)  
	}
}
