using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NabfProject.AI;
using System.Reflection;
using JSLibrary.Data;
using NabfProject.KnowledgeManager;

namespace NabfTest
{
    [TestFixture]
    public class KnowledgeManagerTest
    {
        KnowledgeManager km;
        NabfAgent agent1, agent2, agent3, agent4;
        NodeKnowledge nk1, nk2;
        EdgeKnowledge ek;
        public int evtTriggered;
        public List<Knowledge> k;

        [SetUp]
        public void Initialization()
        {
            km = new KnowledgeManager();
            agent1 = new NabfAgent("agent1");
            agent2 = new NabfAgent("agent2");
            agent3 = new NabfAgent("agent3");
            agent4 = new NabfAgent("agent4");
            nk1 = new NodeKnowledge("vertex1", 0);
            nk2 = new NodeKnowledge("vertex2", 0);
            ek = new EdgeKnowledge("vertex1", "vertex2", 0);

            evtTriggered = 0;
            k = new List<Knowledge>();
        }


        [Test]
        public void SubscribeAgent_NoDuplicate_Success()
        {
            km.Subscribe(agent1);
            object f = getField(km, false, "_sharingList");
            HashSet<NabfAgent> sharingList = (HashSet<NabfAgent>)f;
            Assert.AreEqual(1, sharingList.Count);

            km.Subscribe(agent2);
            f = getField(km, false, "_sharingList");
            sharingList = (HashSet<NabfAgent>)f;
            Assert.AreEqual(2, sharingList.Count);
        }

        [Test]
        public void SubscribeAgent_Duplicate_Failure()
        {
            km.Subscribe(agent1);
            km.Subscribe(agent2);
            km.Subscribe(agent1);
            object f = getField(km, false, "_sharingList");
            HashSet<NabfAgent> sharingList = (HashSet<NabfAgent>)f;
            Assert.AreEqual(2, sharingList.Count);
        }

        [Test]
        public void UnsubscribeAgent_Exists_Success()
        {
            km.Subscribe(agent1);
            km.Subscribe(agent2);
            object f = getField(km, false, "_sharingList");
            HashSet<NabfAgent> sharingList = (HashSet<NabfAgent>)f;
            Assert.AreEqual(2, sharingList.Count);

            km.Unsubscribe(agent1);
            f = getField(km, false, "_sharingList");
            sharingList = (HashSet<NabfAgent>)f;
            Assert.AreEqual(1, sharingList.Count);
        }

        [Test]
        public void UnsubscribeAgent_DontExists_Failure()
        {
            km.Subscribe(agent1);
            km.Subscribe(agent2);
            object f = getField(km, false, "_sharingList");
            HashSet<NabfAgent> sharingList = (HashSet<NabfAgent>)f;
            Assert.AreEqual(2, sharingList.Count);

            bool b = km.Unsubscribe(agent3);
            f = getField(km, false, "_sharingList");
            sharingList = (HashSet<NabfAgent>)f;
            Assert.AreEqual(2, sharingList.Count);
            Assert.IsFalse(b);
        }

        [Test]
        public void SendKnowledgeMixed_DontExists_Success()
        {
            return;
            km.Subscribe(agent1);
            km.Subscribe(agent2);
            km.Subscribe(agent3);
            km.SendKnowledgeToManager(new List<Knowledge>() { nk1, ek }, agent1);
            km.SendKnowledgeToManager(new List<Knowledge>() { nk2 }, agent2);

            agent1.EventManager = new XmasEngineModel.Management.EventManager();
            agent2.EventManager = new XmasEngineModel.Management.EventManager();
            agent3.EventManager = new XmasEngineModel.Management.EventManager();

            act<NewKnowledgeEvent> a = new act<NewKnowledgeEvent>();
            agent1.Register(new XmasEngineModel.Management.Trigger<NewKnowledgeEvent>(a));

            //setup event listener
            {
                //k.Add(knowledge);
                evtTriggered++;
            }

            Assert.AreEqual(6, evtTriggered);
            Assert.IsTrue(k[0].Equals(nk1));
            Assert.IsTrue(k[1].Equals(nk1));
            Assert.IsTrue(k[2].Equals(ek));
            Assert.IsTrue(k[3].Equals(ek));

            Assert.IsTrue(k[4].Equals(nk2));
            Assert.IsTrue(k[5].Equals(nk2));
        }

        [Test]
        public void SendKnowledgeMixed_Exists_Failure()
        {
            return;
            km.Subscribe(agent1);
            km.Subscribe(agent2);
            km.Subscribe(agent3);
            km.SendKnowledgeToManager(new List<Knowledge>() { nk1, ek }, agent1);
            km.SendKnowledgeToManager(new List<Knowledge>() { ek, nk1 }, agent2);

            int evtTriggered = 0;
            List<Knowledge> k = new List<Knowledge>();
            //setup event listener 
            {
                //k.Add(knowledge);
                evtTriggered++;
            }

            Assert.AreEqual(4, evtTriggered);
            Assert.IsTrue(k[0].Equals(nk1));
            Assert.IsTrue(k[1].Equals(nk1));
            Assert.IsTrue(k[2].Equals(ek));
            Assert.IsTrue(k[3].Equals(ek));
        }

        private void dostuff()
        {
        }

        private class act<NewKnowledgeEvent> : XmasEngineModel.Management.EntityXmasAction<NewKnowledgeEvent>
        {
            protected override void Execute()
            {
                throw new NotImplementedException();
            }
        }

        private class TestEventTrigger : XmasEngineModel.Management.Trigger
        {
            private ICollection<Type> _events;
            private KnowledgeManagerTest _kmt;

            public TestEventTrigger(ICollection<Type> eves, KnowledgeManagerTest kmt)
            {
                _events = eves;
                _kmt = kmt;
            }

            public override ICollection<Type> Events
            {
                get { return _events; }
            }

            protected override bool CheckCondition(XmasEngineModel.Management.XmasEvent evt)
            {
                return true;
            }

            protected override void Execute(XmasEngineModel.Management.XmasEvent evt)
            {
                _kmt.evtTriggered++;
                _kmt.k.Add(((NewKnowledgeEvent)evt).NewKnowledge);
            }
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
