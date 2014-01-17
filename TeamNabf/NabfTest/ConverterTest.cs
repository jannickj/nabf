using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NabfProject.AI;
using System.IO;
using NabfProject.ServerMessages;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;
using NabfProject.Parsers;
using JSLibrary.IiLang;
using JSLibrary.IiLang.DataContainers;

namespace NabfTest
{
    [TestFixture]
    public class ConverterTest
    { 
        ServerCommunication serverCom;
        MemoryStream memoryStream;
        StreamReader sReader;
        StreamWriter sWriter;

        [SetUp]
        public void Init()
        {
            memoryStream = new MemoryStream();
            memoryStream.Position = 0;
            sReader = new StreamReader(memoryStream);
            sWriter = new StreamWriter(memoryStream);
            serverCom = new ServerCommunication(sReader, sWriter);
        }

        
        [Test]
        public void ParseRequestAction_FullActionRequest_Success()
        {
            long timestamp = 1297263230578;
            long perception_deadline = 1297263232578;
            int perception_id = 201;
            int simulation_step = 200;
            int self_energy = 19;
            int self_health = 9;
            string self_lastAction = "skip";
            string self_lastActionParam = "";
            string self_lastActionResult = "successful";
            int self_maxEnergy = 19;
            int self_maxEnergyDisabled = 9;
            int self_maxHealth = 9;
            string self_position = "vertex4";
            int self_strength = 5;
            int self_visRange = 5;
            int self_zoneScore = 27;
            int team_lastStepScore = 27;
            int team_money = 1;
            int team_score = 4270;
            int team_zonesScore = 26;
            string achievement_name = "area20";
            string visibleVertex_name = "vertex19";
            string visibleVertex_team = "none";
            string visibleEdge_node1 = "vertex0";
            string visibleEdge_node2 = "vertex11";
            string visibleEntity_status = "normal";
            int probedVertex_value = 4;
            int surveyedEdge_weight = 2;
            int inspectedEntity_maxHealth = 9;

            sWriter.Write("<message timestamp=\"" + timestamp + "\" type=\"request-action\">"
                            + "<perception deadline=\""+perception_deadline+"\" id=\""+perception_id+"\">"
                            + "<simulation step=\""+simulation_step+"\"/>"
                            + "<self energy=\""+self_energy+"\" health=\""+self_health+"\" lastAction=\""+self_lastAction+"\" " 
                            + "lastActionParam=\""+self_lastActionParam+"\" lastActionResult=\""+self_lastActionResult+"\" maxEnergy=\""+self_maxEnergy+"\" "
                            + "maxEnergyDisabled=\""+self_maxEnergyDisabled+"\" maxHealth=\""+self_maxHealth+"\" position=\""+self_position+"\" "
                            + "strength=\""+self_strength+"\" visRange=\""+self_visRange+"\" zoneScore=\""+self_zoneScore+"\"/>"
                            + "<team lastStepScore=\""+team_lastStepScore+"\" money=\""+team_money+"\" score=\""+team_score+"\" zonesScore=\""+team_zonesScore+"\">"
                            + "<achievements>"
                            + "<achievement name=\""+achievement_name+"\"/>"
                            + "</achievements>"
                            + "</team>"
                            + "<visibleVertices>"
                            + "<visibleVertex name=\""+visibleVertex_name+"\" team=\""+visibleVertex_team+"\"/>"
                            + "</visibleVertices>"
                            + "<visibleEdges>"
                            + "<visibleEdge node1=\""+visibleEdge_node1+"\" node2=\""+visibleEdge_node2+"\"/>"
                            + "</visibleEdges>"
                            + "<visibleEntities>"
                            + "<visibleEntity name=\"b5\" team=\"B\" node=\"vertex0\" "
                            + "status=\""+visibleEntity_status+"\"/>"
                            + "</visibleEntities>"
                            + "<probedVertices>"
                            + "<probedVertex name=\"vertex18\" value=\""+probedVertex_value+"\"/>"
                            + "</probedVertices>"
                            + "<surveyedEdges>"
                            + "<surveyedEdge node1=\"vertex3\" node2=\"vertex7\" weight=\""+surveyedEdge_weight+"\"/>"
                            + "</surveyedEdges>"
                            + "<inspectedEntities>"
                            + "<inspectedEntity energy=\"8\" health=\"9\" maxEnergy=\"8\" "
                            + "maxHealth=\""+inspectedEntity_maxHealth+"\" name=\"b5\" node=\"vertex10\" role=\"role2\" "
                            + "strength=\"6\" team=\"B\" visRange=\"2\"/>"
                            + "</inspectedEntities>"
                            + "</perception>"
                            + "</message>");

            sWriter.Flush();
            sWriter.BaseStream.WriteByte(0);
            sWriter.BaseStream.Flush();

            memoryStream.Position = 0;

            sReader = new StreamReader(memoryStream);
            serverCom = new ServerCommunication(sReader, sWriter);

            ReceiveMessage package = (ReceiveMessage)serverCom.DeserializePacket();

            MarsToAgentParser mtap = new MarsToAgentParser();

            IilPerceptCollection ipc = (IilPerceptCollection) mtap.ConvertToForeign(package);

            Assert.AreEqual("action_request", ipc.Percepts[0].Name);

        }


     
    }
    
}
