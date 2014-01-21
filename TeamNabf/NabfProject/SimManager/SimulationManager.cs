using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NabfProject.KnowledgeManagerModel;
using NabfProject.NoticeBoardModel;
using NabfProject.AI;
using NabfProject.Events;

namespace NabfProject.SimManager
{
    public class SimulationManager
    {
        private Dictionary<int, SimulationData> _simDataStorage = new Dictionary<int, SimulationData>();
        private SimulationFactory _factory;
        private int _currentID;

        public SimulationManager(SimulationFactory sf)
        {
            _factory = sf;
        }

        private bool TryGetNoticeBoard(int simID, out NoticeBoard nb)
        {
            KnowledgeManager km;
            TryGetSimData(simID, out km, out nb);
            return true;
        }
        private bool TryGetKnowledgeManager(int simID, out KnowledgeManager km)
        {
            NoticeBoard nb;
            TryGetSimData(simID, out km, out nb);
            return true;
        }
        private bool TryGetSimData(int simID, out KnowledgeManager km, out NoticeBoard nb)
        {
            bool b = false;
            SimulationData sd;
            km = null;
            nb = null;

            b = _simDataStorage.TryGetValue(simID, out sd);
            if (b == false)
                throw new ArgumentException("id " + _currentID + " not found.");
                //return false;

            km = sd.KnowledgeManager;
            nb = sd.NoticeBoard;

            return b;
        }

        private bool TryInsertSimData(int simID)
        {
            bool b = true;

            try
            {
                _simDataStorage.Add(simID, _factory.ContructSimulationData());
            }
            catch
            {
                b = false;
            }

            return b;
        }

        public void SubscribeToSimulation(int simID, NabfAgent agent)
        {
            KnowledgeManager km;
            NoticeBoard nb;
            if (_currentID != simID)
            {
                TryGetSimData(_currentID, out km, out nb);
                km.Unsubscribe(agent);
                nb.Unsubscribe(agent);

                TryInsertSimData(simID);
                _currentID = simID;
            }

            TryGetSimData(simID, out km, out nb);
            km.Subscribe(agent);
            nb.Subscribe(agent);
            agent.Raise(new SimulationSubscribedEvent(simID));
        }

        public void SendKnowledge(int id, List<Knowledge> sentKnowledge, NabfAgent sender)
        {
            if (id != _currentID)
                return;

            KnowledgeManager km;
            TryGetKnowledgeManager(id, out km);

            km.SendKnowledgeToManager(sentKnowledge, sender);
        }

        public bool CreateAndAddNotice(int simID, NoticeBoard.JobType type, int agentsNeeded, List<NodeKnowledge> whichNodes, int value, out Notice notice)
        {
            NoticeBoard nb;
            TryGetNoticeBoard(simID, out nb);

            bool ret = nb.CreateAndAddNotice(type, agentsNeeded, whichNodes, value, out notice);

            return ret;
        }

        public bool RemoveNotice(int simID, Notice no)
        {
            if (_currentID != simID)
                return false;
            NoticeBoard nb;
            TryGetNoticeBoard(simID, out nb);

            return nb.RemoveNotice(no);
        }

        public bool UpdateNotice(int simID, Int64 noticeID, int agentsNeeded, List<NodeKnowledge> whichNodes, int value)
        {
            if (_currentID != simID)
                return false;
            NoticeBoard nb;
            TryGetNoticeBoard(simID, out nb);

            return nb.UpdateNotice(noticeID, whichNodes, agentsNeeded, value);               
        }

        public NoticeBoard.JobType NoticeToJobType(Notice no)
        {
            NoticeBoard nb;
            TryGetNoticeBoard(_currentID, out nb);

            return nb.NoticeToJobType(no);
        }

        public void ApplyToNotice(int simID, Notice notice, int desirability, NabfAgent a)
        {
            if (_currentID != simID)
                return;
            NoticeBoard nb;
            TryGetNoticeBoard(simID, out nb);

            nb.ApplyToNotice(notice, desirability, a);
        }
        public void UnApplyToNotice(int simID, Notice notice, NabfAgent a)
        {
            if (_currentID != simID)
                return;

            NoticeBoard nb;
            TryGetNoticeBoard(simID, out nb);

            nb.UnApplyToNotice(notice, a);
        }

        public void FindJobsForAgents(int simID)
        {
            if (_currentID != simID)
                return;
            NoticeBoard nb;
            TryGetNoticeBoard(simID, out nb);

            nb.FindJobsForAgents();
        }
    }

    public struct SimulationData
    {
        public KnowledgeManager KnowledgeManager { get; set; }
        public NoticeBoard NoticeBoard { get; set; }
    }
}
