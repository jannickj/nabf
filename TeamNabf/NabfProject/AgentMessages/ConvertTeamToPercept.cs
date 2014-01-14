using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSLibrary.Conversion;
using JSLibrary.IiLang.DataContainers;
using JSLibrary.IiLang.Parameters;
using NabfProject.ServerMessages;

namespace NabfProject.AgentMessages
{
    public class ConvertTeamToPercept : JSConverterToForeign<TeamMessage, IilPercept>
    {
        public override IilPercept BeginConversionToForeign(TeamMessage gobj)
        {
            var achies = gobj.Achievements.AchievementList.OfType<AchievementMessage>().Select(a => new IilIdentifier(a.Name)).ToArray();
            return new IilPercept("team", new IilFunction("lastStepScore", new IilNumeral(gobj.LastStepScore))
                , new IilFunction("money", new IilNumeral(gobj.Money))
                , new IilFunction("score", new IilNumeral(gobj.Score))
                , new IilFunction("zoneScore", new IilNumeral(gobj.ZonesScore))
                , new IilFunction("achievements", achies)); 
        }
    }
}
