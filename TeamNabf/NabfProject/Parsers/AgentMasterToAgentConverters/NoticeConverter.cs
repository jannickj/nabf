using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSLibrary.Conversion;
using JSLibrary.IiLang;
using JSLibrary.IiLang.DataContainers;
using JSLibrary.IiLang.Parameters;
using NabfProject.NoticeBoardModel;

namespace NabfProject.Parsers.NoticeConverters
{
    public class NoticeConverter : JSConverter<Notice, IilElement>
    {
        public override object ForeignID
        {
            get
            {
                return "notice";
            }
        }

        public override IilElement BeginConversionToForeign(Notice gobj)
        {
            string name = gobj.GetType().Name;
            string firstLetter = name.First().ToString().ToLower();
            string jobId = firstLetter.Insert(1, name.Remove(0, 1));

            return new IilPerceptCollection(new IilPercept("notice"
                , new IilFunction("type", new IilIdentifier(jobId))
                , new IilFunction("agentsNeeded", new IilNumeral(gobj.AgentsNeeded))
                , new IilFunction("id", new IilNumeral(gobj.Id))
                , new IilFunction("value", new IilNumeral(gobj.Value))
                , new IilFunction("whichNodes", gobj.WhichNodes.Select(n => (new IilIdentifier(n.Name))))
                ));
        }

        public override Notice BeginConversionToKnown(IilElement fobj)
        {
            throw new NotImplementedException();
        }
    }
}
