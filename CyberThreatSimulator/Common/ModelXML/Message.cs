using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Common.ModelXML.XML
{
    /**
     * interface for all DEMXML classes
     */

    public interface IMessage
    {
        //print to console
        void printToConsole();

        //Serialize to XML String
        string  toXmlString();
    }
}
