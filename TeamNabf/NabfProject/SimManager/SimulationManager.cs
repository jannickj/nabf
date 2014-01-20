using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NabfProject.KnowledgeManagerModel;
using NabfProject.NoticeBoardModel;

namespace NabfProject.SimManager
{
    public class SimulationManager
    {
        private Dictionary<int, SimulationData> _simDataStorage = new Dictionary<int, SimulationData>();
        private SimulationFactory _factory;

        public SimulationManager(SimulationFactory sf)
        {
            _factory = sf;
        }

        public bool TryGetSimData(int simID, out KnowledgeManager km, out NoticeBoard nb)
        {
            bool b = false;
            SimulationData sd;
            km = null;
            nb = null;

            b = _simDataStorage.TryGetValue(simID, out sd);
            if (b == false)
                return b;

            km = sd.KnowledgeManager;
            nb = sd.NoticeBoard;

            return b;
        }

        public bool TryInsertSimData(int simID)
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
        
        //public void 
    }

    struct SimulationData
    {
        public KnowledgeManager KnowledgeManager { get; set; }
        public NoticeBoard NoticeBoard { get; set; }

        //public SimulationData(KnowledgeManager km, NoticeBoard nb)
        //{
        //    KnowledgeManager = km;
        //    NoticeBoard = nb;
        //}
    }
}
