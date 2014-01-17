using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabfProject.ServerMessages
{
    public class TeamMessage : InternalReceiveMessage
    {
        private int lastStepScore;

        public int LastStepScore
        {
            get { return lastStepScore; }
        }
        private int money;

        public int Money
        {
            get { return money; }
        }
        private int score;

        public int Score
        {
            get { return score; }
        }
        private int zonesScore;

        public int ZonesScore
        {
            get { return zonesScore; }
        }

        private string messageName;

        public string MessageName
        {
            get { return messageName; }
        }

        private InternalReceiveMessage achievements;

        public InternalReceiveMessage Achievements
        {
            get { return achievements; }
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            reader.MoveToContent();

            messageName = reader.LocalName;
            lastStepScore = Convert.ToInt32(reader["lastStepScore"]);
            money = Convert.ToInt32(reader["money"]);
            score = Convert.ToInt32(reader["score"]);
            zonesScore = Convert.ToInt32(reader["zonesScore"]);

            reader.Read();
            if ("achievements" == reader.LocalName)
            {
                reader.MoveToContent();
                var message = ServerMessageFactory.Instance.ConstructMessage(reader.LocalName);
                message.ReadXml(reader);
                achievements = message;

                reader.ReadEndElement();
            }
        }
    }
}
