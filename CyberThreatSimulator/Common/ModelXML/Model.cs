using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.ModelXML.XML;

namespace Common.ModelXML
{
    public interface IModel
    {
        MessageWrapper generateMessageWrapper(string type);
    }
}
