using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabfProject.ServerMessages
{
    public class SimStartMessage : InternalReceiveMessage
    {
        

        private Dictionary<string, string> response = new Dictionary<string, string>();
        public Dictionary<string, string> Response { get { return response; } }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();

            string edges = reader["edges"];
            if (edges != null && edges != "")
                response.Add("edges", edges);

            string id = reader["id"];
            if (id != null && id != "")
                response.Add("id", id);

            string steps = reader["steps"];
            if (steps != null && steps != "")
                response.Add("steps", steps);

            string vertices = reader["vertices"];
            if (vertices != null && vertices != "")
                response.Add("vertices", vertices);

            string role = reader["role"];
            if (role != null && role != "")
                response.Add("role", role);

            reader.Read();
            reader.MoveToContent();
        }

        
    }
}
