using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabfProject.ServerMessages
{
    public class SimEndMessage : InternalReceiveMessage
    {
        private Dictionary<string, string> response = new Dictionary<string, string>();
        public Dictionary<string, string> Response { get { return response; } }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();

            string ranking = reader["ranking"];
            if (ranking != null && ranking != "")
                response.Add("ranking", ranking);

            string score = reader["score"];
            if (score != null && score != "")
                response.Add("score", score);
                       
            reader.Read();
        }

        
    }
}
