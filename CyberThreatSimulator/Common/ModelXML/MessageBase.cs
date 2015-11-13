using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Common.ModelXML.XML
{
    /**
     * Super class for all Messages in and out of the network pipe
     */
    [Serializable]
    public abstract class MessageBase : IMessage
    {

        //Message types
        public const string MESSAGE_NOTIFCATION = "NOTIFICATION";
        public const string MESSAGE_JOIN_REQUEST = "JOIN_REQUEST"; //an app would like to join the sim
        public const string MESSAGE_LEAVE_REQUEST = "LEAVE_REQUEST"; //an app would like to leave the sim
        public const string MESSAGE_UPDATE = "UPDATE"; //terrain updatesetc.
        public const string MESSAGE_UPDATE_PARTICPANT_LIST = "UPDATE_LIST";
        public const string MESSAGE_SIM_SETUP = "SIM_SETUP"; //terrain updates etc.
        public const string MESSAGE_SIM_TERMINATE = "SIM_FINISH"; //terrain updates, threat updates, etc.
        public const string MESSAGE_JOIN_ACCEPTANCE = "JOIN_ACCEPTANCE"; //terrain updates etc.
        public const string MESSAGE_SIM_APP_CONFIG = "SIM_APP_CONFIG"; //terrain updates etc.


        [XmlElement("MessageID")]
        public string MessageId { get; set; } //this message
        [XmlElement("AppRole")]
        public string ApplicationRole { get; set; } //this applications Role: Manager, Participant...etc
        [XmlElement("AppID")]
        public string ApplicationId { get; set; } //the unique Id of the application
        [XmlElement("MessageType")]
        public string MessageType { get; set; } //the type of message
        [XmlElement("Timestamp")]
        public DateTime Time { get; set; } //this messages creation time

        public MessageBase()
        {
            Time = DateTime.Now;
            MessageId = Guid.NewGuid().ToString().Replace("-","").Substring(0, 15).ToUpper();
        }

        public string toXmlString()
        {
            //force the necessary message parameters to exist for all messages before sending, essentially,
            if (String.IsNullOrEmpty(MessageId)) throw new NullReferenceException("MessageBase: Message Id cannot be null");
            if (String.IsNullOrEmpty(ApplicationId)) throw new NullReferenceException("MessageBase: Application Id cannot be null");
            if (String.IsNullOrEmpty(ApplicationRole)) throw new NullReferenceException("MessageBase: Application Role cannot be null");
            if (String.IsNullOrEmpty(MessageType)) throw new NullReferenceException("MessageBase: Message Type cannot be null");
            if (Time == null) throw new NullReferenceException("MessageBase: Time cannot be null");

            XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }
        }

        public void printToConsole()
        {
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(this.GetType());
            x.Serialize(Console.Out, this);
            Console.WriteLine();
        }


    }
}
