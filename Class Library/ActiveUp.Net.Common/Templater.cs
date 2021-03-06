// Copyright 2001-2010 - Active Up SPRLU (http://www.agilecomponents.com)
//
// This file is part of MailSystem.NET.
// MailSystem.NET is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// MailSystem.NET is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Net;

namespace ActiveUp.Net.Mail
{
    /// <summary>
    /// Templater class to create a mail with it's settings using a single XML file.
    /// </summary>
#if !PocketPC
    [Serializable]
#endif
    public class Templater
    {
        private Message _message;
        private FieldFormatCollection _fieldsFormats;
        private ConditionalCollection _conditions;
        private RegionCollection _regions;
        private ServerCollection _smtpServers;
        private ListTemplateCollection _listTemplates;

        /// <summary>
        /// Gets or sets the logging settings.
        /// </summary>
        public Message Message
        {
            get { return _message ?? (_message = new Message()); }
            set { _message = value; }
        }

        /// <summary>
        /// The default constructor.
        /// </summary>
        public Templater()
        {
        }

        /// <summary>
        /// Instanciate the templater and directly load the template file.
        /// </summary>
        /// <param name="templateFile">the filepath to the template file.</param>
        public Templater(string templateFile)
        {
            LoadTemplate(templateFile);
        }

        /// <summary>
        /// Load a template from a file.
        /// </summary>
        /// <param name="filename">The fullpath to the file.</param>
        public void LoadTemplate(string filename)
        {
            Logger.AddEntry(GetType(), "Loading template " + filename + ".", 2);
            string fileContent = LoadFileContent(filename);
            Logger.AddEntry(GetType(), "Template length: " + fileContent.Length.ToString() + " bytes.", 1);

            if (fileContent.Length > 0)
                ProcessXmlTemplate(fileContent);
            else
                throw new Exception("The specified file is empty or not valid.");
        }

        /// <summary>
        /// Load a template from a string.
        /// </summary>
        /// <param name="content">The template string.</param>
        public void LoadTemplateFromString(string content)
        {
            Logger.AddEntry(GetType(), "Loading template from string.", 2);
            Logger.AddEntry(GetType(), "Template length: " + content.Length.ToString() + " bytes.", 1);
            ProcessXmlTemplate(content);
        }

        /*/// <summary>
        /// Process the Text template.
        /// </summary>
        private void ProcessTextTemplate(string content)
        {
            ActiveUp.Net.Mail.Logger.AddEntry("Processing the TEXT template.", 1);

            // Initialize strings to be used later
            string line = string.Empty, lineUpper = string.Empty;

            // Initialize the StringReader to read line per line
            StringReader reader = new StringReader(content);

            // Initialize the actual body count
            int bodyCount = _bodies.Count, lineNumber = 0;

            // Read and parse each line. Append the data in the properties.
            while (reader.Peek() > -1) 
            {
                ActiveUp.Net.Mail.Logger.AddEntry("Line parsed. Body count: + " + bodyCount.ToString() + ".", 0);

                line = reader.ReadLine();
                lineNumber++;
                lineUpper = line.ToUpper();
                
                // If a property, then set value    
                if (lineUpper.StartsWith("TO:"))
                {
                    ActiveUp.Net.Mail.Logger.AddEntry("TO property found: + " + line + " (raw).", 0);
                    this.Message.To.Add(Parser.ParseAddress(ExtractValue(line)));
                }
                else if (lineUpper.StartsWith("BCC:"))
                {
                    ActiveUp.Net.Mail.Logger.AddEntry("BCC property found: + " + line + " (raw).", 0);
                    this.Message.Bcc.Add(Parser.ParseAddress(ExtractValue(line)));
                }
                else if (lineUpper.StartsWith("CC:"))
                {
                    ActiveUp.Net.Mail.Logger.AddEntry("CC property found: + " + line + " (raw).", 0);
                    this.Message.Cc.Add(Parser.ParseAddress(ExtractValue(line)));
                }
                else if (lineUpper.StartsWith("FROM:"))
                {
                    ActiveUp.Net.Mail.Logger.AddEntry("FROM property found: + " + line + " (raw).", 0);
                    this.Message.From = Parser.ParseAddress(ExtractValue(line));
                }
                else if (lineUpper.StartsWith("SUBJECT:"))
                {
                    ActiveUp.Net.Mail.Logger.AddEntry("SUBJECT property found: + " + line + " (raw).", 0);
                    this.Message.Subject += ExtractValue(line);
                }
                else if (lineUpper.StartsWith("SMTPSERVER:"))
                {
                    ActiveUp.Net.Mail.Logger.AddEntry("SMTPSERVER property found: + " + line + " (raw).", 0);
                    this.SmtpServers.Add(ExtractValue(line), 25);
                }
                else if (lineUpper.StartsWith("BODYTEXT:"))
                {
                    ActiveUp.Net.Mail.Logger.AddEntry("BODYTEXT property found: + " + line + " (raw).", 0);
                    this.Bodies.Add(ExtractValue(line), BodyFormat.Text);
                    bodyCount++;
                }
                else if (lineUpper.StartsWith("BODYHTML:"))
                {
                    ActiveUp.Net.Mail.Logger.AddEntry("BODYHTML property found: + " + line + " (raw).", 0);
                    this.Bodies.Add(ExtractValue(line), BodyFormat.Html);
                    bodyCount++;
                }
                else if (lineUpper.StartsWith("FIELDFORMAT:") && lineUpper.IndexOf("=") > -1)
                {
                    ActiveUp.Net.Mail.Logger.AddEntry("FIELDFORMAT property found: + " + line + " (raw).", 0);
                    this.FieldsFormats.Add(ExtractFormat(line));
                }
                else if (lineUpper.StartsWith("//"))
                {
                    ActiveUp.Net.Mail.Logger.AddEntry("COMMENT line found: + " + line + " (raw).", 0);
                    // Line is a comment, so do nothing
                }
                    // If not a property, then it's a message line
                else
                {
                    ActiveUp.Net.Mail.Logger.AddEntry("BODY line found: + " + line + " (raw).", 0);
                    this.Bodies[bodyCount-1].Content += line + "\r\n";                    
                }
            }
        }*/

        /*/// <summary>
        /// Extract the format options from a text template line.
        /// </summary>
        /// <param name="line">The text template line.</param>
        /// <returns>A FieldFormat object with the options.</returns>
        private FieldFormat ExtractFormat(string line)
        {
            ActiveUp.Net.Mail.Logger.AddEntry("Extracting FieldFormat from line: + " + line + " (raw).", 0);
            
            FieldFormat fieldFormat = new FieldFormat();
            string property, val;

            foreach(string format in ExtractValue(line).Split(';'))
            {
                string[] lineSplit = format.Split('=');
                
                if (lineSplit.Length > 1)
                {
                    property = lineSplit[0];
                    val = lineSplit[1];

                    switch (property.ToUpper())
                    {
                        case "NAME": fieldFormat.Name = val; break;
                        case "FORMAT": fieldFormat.Format = val; break;
                        case "PADDINGDIR":
                            if (val.ToUpper() == "LEFT")
                                fieldFormat.PaddingDir = PaddingDirection.Left; 
                            else
                                fieldFormat.PaddingDir = PaddingDirection.Right;
                            break;
                        case "TOTALWIDTH":
                            try
                            {
                                fieldFormat.TotalWidth = Convert.ToInt16(val);
                            }
                            catch
                            {
                                throw new Exception("Specified Total Width is not a valid number.");
                            }
                            break;
                        case "PADDINGCHAR": fieldFormat.PaddingChar = Convert.ToChar(val.Substring(0, 1)); break;
                    }

                }// End if line split length > 1
            }

            return fieldFormat;
        }*/

        /// <summary>
        /// Process the Xml template.
        /// </summary>
        private void ProcessXmlTemplate(string content)
        {
            Logger.AddEntry(GetType(), "Processing the XML template.", 1);

            StringReader stringReader = new StringReader(content);
            XmlTextReader reader = new XmlTextReader(stringReader);

            string element = string.Empty;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        element = reader.Name;
                        Logger.AddEntry(GetType(), "New element found: " + element + ".", 0);
                        switch (element.ToUpper())
                        {
                            case "MESSAGE":
                                if (reader.GetAttribute("PRIORITY") != null && reader.GetAttribute("PRIORITY") != string.Empty)
                                    Message.Priority = (MessagePriority)Enum.Parse(typeof(MessagePriority), reader.GetAttribute("PRIORITY"), true); 
                                else if (reader.GetAttribute("priority") != null && reader.GetAttribute("priority") != string.Empty)
                                    Message.Priority = (MessagePriority)Enum.Parse(typeof(MessagePriority), reader.GetAttribute("priority"), true); 
                                break;
                            case "FIELDFORMAT":
                                if (reader.HasAttributes)
                                {
                                    Logger.AddEntry(GetType(), "Element has attributes.", 0);
                                    FieldFormat fieldFormat = new FieldFormat();
                                    if (reader.GetAttribute("NAME") != null && reader.GetAttribute("NAME") != string.Empty)
                                    {
                                        fieldFormat.Name = reader.GetAttribute("NAME");
                                        Logger.AddEntry(GetType(), "Attribute NAME: " + fieldFormat.Name, 0);
                                    }
                                    else if (reader.GetAttribute("name") != null && reader.GetAttribute("name") != string.Empty)
                                    {
                                        fieldFormat.Name = reader.GetAttribute("name");
                                        Logger.AddEntry(GetType(), "Attribute name: " + fieldFormat.Name, 0);
                                    }
                                    if (reader.GetAttribute("FORMAT") != null && reader.GetAttribute("FORMAT") != string.Empty)
                                    {
                                        fieldFormat.Format = reader.GetAttribute("FORMAT");
                                        Logger.AddEntry(GetType(), "Attribute FORMAT: " + fieldFormat.Format, 0);
                                    }
                                    else if (reader.GetAttribute("format") != null && reader.GetAttribute("format") != string.Empty)
                                    {
                                        fieldFormat.Format = reader.GetAttribute("format");
                                        Logger.AddEntry(GetType(), "Attribute format: " + fieldFormat.Format, 0);
                                    }
                                    if (reader.GetAttribute("PADDINGDIR") != null && reader.GetAttribute("PADDINGDIR") != string.Empty)
                                    {
                                        fieldFormat.PaddingDir = reader.GetAttribute("PADDINGDIR").ToUpper() == "LEFT" ? PaddingDirection.Left : PaddingDirection.Right;
                                        Logger.AddEntry(GetType(), "Attribute PADDINGDIR: " + reader.GetAttribute("PADDINGDIR"), 0);
                                    }
                                    else if (reader.GetAttribute("paddingdir") != null && reader.GetAttribute("paddingdir") != string.Empty)
                                    {
                                        fieldFormat.PaddingDir = reader.GetAttribute("paddingdir").ToUpper() == "left" ? PaddingDirection.Left : PaddingDirection.Right;
                                        Logger.AddEntry(GetType(), "Attribute paddingdir: " + reader.GetAttribute("paddingdir"), 0);
                                    }
                                    if (reader.GetAttribute("TOTALWIDTH") != null && reader.GetAttribute("TOTALWIDTH") != string.Empty)
                                    {
                                        try
                                        {
                                            fieldFormat.TotalWidth = Convert.ToInt16(reader.GetAttribute("TOTALWIDTH"));
                                        }
                                        catch
                                        {
                                            throw new Exception("Specified Total Width is not a valid number.");
                                        }
                                        Logger.AddEntry(GetType(), "Attribute TOTALWIDTH: " + fieldFormat.TotalWidth.ToString(), 0);
                                    }
                                    else if (reader.GetAttribute("totalwidth") != null && reader.GetAttribute("totalwidth") != string.Empty)
                                    {
                                        try
                                        {
                                            fieldFormat.TotalWidth = Convert.ToInt16(reader.GetAttribute("totalwidth"));
                                        }
                                        catch
                                        {
                                            throw new Exception("Specified Total Width is not a valid number.");
                                        }
                                        Logger.AddEntry(GetType(), "Attribute totalwidth: " + fieldFormat.TotalWidth.ToString(), 0);
                                    }
                                    if (reader.GetAttribute("PADDINGCHAR") != null && reader.GetAttribute("PADDINGCHAR") != string.Empty)
                                    {
                                        fieldFormat.PaddingChar = Convert.ToChar(reader.GetAttribute("PADDINGCHAR").Substring(0, 1));
                                        Logger.AddEntry(GetType(), "Attribute PADDINGCHAR: '" + fieldFormat.PaddingChar + "'", 0);
                                    }
                                    else if (reader.GetAttribute("paddingchar") != null && reader.GetAttribute("paddingchar") != string.Empty)
                                    {
                                        fieldFormat.PaddingChar = Convert.ToChar(reader.GetAttribute("paddingchar").Substring(0, 1));
                                        Logger.AddEntry(GetType(), "Attribute paddingchar: '" + fieldFormat.PaddingChar + "'", 0);
                                    }
                                    FieldsFormats.Add(fieldFormat);
                                }
                                break;
                            case "FROM":
                            case "TO":
                            case "CC":
                            case "BCC":
                                if (reader.HasAttributes)
                                {
                                    Address address = new Address();
                                    if (reader.GetAttribute("NAME") != null && reader.GetAttribute("NAME") != string.Empty)
                                        address.Name = reader.GetAttribute("NAME");
                                    else if (reader.GetAttribute("name") != null && reader.GetAttribute("name") != string.Empty)
                                        address.Name = reader.GetAttribute("name");
                                    if (reader.GetAttribute("EMAIL") != null && reader.GetAttribute("EMAIL") != string.Empty)
                                        address.Email = reader.GetAttribute("EMAIL");
                                    else if (reader.GetAttribute("email") != null && reader.GetAttribute("email") != string.Empty)
                                        address.Email = reader.GetAttribute("email");
                                    if (element.ToUpper() == "FROM")
                                    {
                                        if (reader.GetAttribute("REPLYNAME") != null && reader.GetAttribute("REPLYNAME") != string.Empty)
                                        {
                                            InitReplyTo();
                                            Message.ReplyTo.Name = reader.GetAttribute("REPLYNAME");
                                        }
                                        else if (reader.GetAttribute("replyname") != null && reader.GetAttribute("replyname") != string.Empty)
                                        {
                                            InitReplyTo();
                                            Message.ReplyTo.Name = reader.GetAttribute("replyname");
                                        }
                                        if (reader.GetAttribute("REPLYEMAIL") != null && reader.GetAttribute("REPLYEMAIL") != string.Empty)
                                        {
                                            InitReplyTo();
                                            Message.ReplyTo.Email = reader.GetAttribute("REPLYEMAIL");
                                        }
                                        else if (reader.GetAttribute("replyemail") != null && reader.GetAttribute("replyemail") != string.Empty)
                                        {
                                            InitReplyTo();
                                            Message.ReplyTo.Email = reader.GetAttribute("replyemail");
                                        }
                                        if (reader.GetAttribute("RECEIPTEMAIL") != null && reader.GetAttribute("RECEIPTEMAIL") != string.Empty)
                                        {
                                            Message.ReturnReceipt.Email = reader.GetAttribute("RECEIPTEMAIL");
                                        }
                                        else if (reader.GetAttribute("receiptemail") != null && reader.GetAttribute("receiptemail") != string.Empty)
                                        {
                                            Message.ReturnReceipt.Email = reader.GetAttribute("receiptemail");
                                        }
                                    }
                                    switch (reader.Name.ToUpper())
                                    {
                                        case "FROM":
                                            Message.From = address;
                                            break;
                                        case "TO":
                                            Message.To.Add(address);
                                            break;
                                        case "CC":
                                            Message.Cc.Add(address);
                                            break;
                                        case "BCC":
                                            Message.Bcc.Add(address);
                                            break;
                                    }
                                }
                                break;
                            case "LISTTEMPLATE":
                                ListTemplate template = new ListTemplate(); 
                                string RegionID = string.Empty;
                                string NullText = string.Empty;
                                if (reader.GetAttribute("REGIONID") != null && reader.GetAttribute("REGIONID") != string.Empty)
                                    RegionID = reader.GetAttribute("REGIONID");
                                else if (reader.GetAttribute("regionid") != null && reader.GetAttribute("regionid") != string.Empty)
                                    RegionID = reader.GetAttribute("regionid");

                                if (reader.GetAttribute("NULLTEXT") != null && reader.GetAttribute("NULLTEXT") != string.Empty)
                                    NullText = reader.GetAttribute("NULLTEXT");
                                else if (reader.GetAttribute("nulltext") != null && reader.GetAttribute("nulltext") != string.Empty)
                                    NullText = reader.GetAttribute("nulltext");

                                if (reader.HasAttributes && reader.GetAttribute("NAME") != null && reader.GetAttribute("NAME") != string.Empty)
                                    template = new ListTemplate(reader.GetAttribute("NAME"), reader.ReadString());
                                else if (reader.HasAttributes && reader.GetAttribute("name") != null && reader.GetAttribute("name") != string.Empty)
                                    template = new ListTemplate(reader.GetAttribute("name"), reader.ReadString());
                                template.RegionID = RegionID;
                                template.NullText = NullText;
                                ListTemplates.Add(template);
                                break;
                            case "SMTPSERVER":
                                Server server = new Server(); 
                                if (reader.GetAttribute("SERVER") != null && reader.GetAttribute("SERVER") != string.Empty)
                                    server.Host = reader.GetAttribute("SERVER");
                                else if (reader.GetAttribute("server") != null && reader.GetAttribute("server") != string.Empty)
                                    server.Host = reader.GetAttribute("server");

                                if (reader.GetAttribute("PORT") != null && reader.GetAttribute("PORT") != string.Empty)
                                    server.Port = int.Parse(reader.GetAttribute("PORT"));
                                else if (reader.GetAttribute("port") != null && reader.GetAttribute("port") != string.Empty)
                                    server.Port = int.Parse(reader.GetAttribute("port"));

                                if (reader.GetAttribute("USERNAME") != null && reader.GetAttribute("USERNAME") != string.Empty)
                                    server.Username = reader.GetAttribute("USERNAME");
                                else if (reader.GetAttribute("username") != null && reader.GetAttribute("username") != string.Empty)
                                    server.Username = reader.GetAttribute("username");

                                if (reader.GetAttribute("PASSWORD") != null && reader.GetAttribute("PASSWORD") != string.Empty)
                                    server.Password = reader.GetAttribute("PASSWORD");
                                else if (reader.GetAttribute("password") != null && reader.GetAttribute("password") != string.Empty)
                                    server.Password = reader.GetAttribute("password");    
                                SmtpServers.Add(server);
                                break;
                            case "CONDITION":
                                Condition condition = new Condition(); 

                                if (reader.GetAttribute("REGIONID") != null && reader.GetAttribute("REGIONID") != string.Empty)
                                    condition.RegionID = reader.GetAttribute("REGIONID");
                                else if (reader.GetAttribute("regionid") != null && reader.GetAttribute("regionid") != string.Empty)
                                    condition.RegionID = reader.GetAttribute("regionid");

                                if (reader.GetAttribute("OPERATOR") != null && reader.GetAttribute("OPERATOR") != string.Empty)
                                    condition.Operator = (OperatorType)Enum.Parse(typeof(OperatorType), reader.GetAttribute("OPERATOR"), true); 
                                else if (reader.GetAttribute("operator") != null && reader.GetAttribute("operator") != string.Empty)
                                    condition.Operator = (OperatorType)Enum.Parse(typeof(OperatorType), reader.GetAttribute("operator"), true); 

                                if (reader.GetAttribute("NULLTEXT") != null && reader.GetAttribute("NULLTEXT") != string.Empty)
                                    condition.NullText = reader.GetAttribute("NULLTEXT");
                                else if (reader.GetAttribute("nulltext") != null && reader.GetAttribute("nulltext") != string.Empty)
                                    condition.NullText = reader.GetAttribute("nulltext");

                                if (reader.GetAttribute("FIELD") != null && reader.GetAttribute("FIELD") != string.Empty)
                                    condition.Field = reader.GetAttribute("FIELD");
                                else if (reader.GetAttribute("field") != null && reader.GetAttribute("field") != string.Empty)
                                    condition.Field = reader.GetAttribute("field");

                                if (reader.GetAttribute("VALUE") != null && reader.GetAttribute("VALUE") != string.Empty)
                                    condition.Value = reader.GetAttribute("VALUE");
                                else if (reader.GetAttribute("value") != null && reader.GetAttribute("value") != string.Empty)
                                    condition.Value = reader.GetAttribute("value");

                                if (reader.GetAttribute("CASESENSITIVE") != null && reader.GetAttribute("CASESENSITIVE") != string.Empty)
                                    condition.CaseSensitive = bool.Parse(reader.GetAttribute("CASESENSITIVE"));
                                else if (reader.GetAttribute("casesensitive") != null && reader.GetAttribute("casesensitive") != string.Empty)
                                    condition.CaseSensitive = bool.Parse(reader.GetAttribute("casesensitive"));
                                Conditions.Add(condition);
                                break;
                            case "REGION":
                                Region region = new Region(); 

                                if (reader.GetAttribute("REGIONID") != null && reader.GetAttribute("REGIONID") != string.Empty)
                                    region.RegionID = reader.GetAttribute("REGIONID");
                                else if (reader.GetAttribute("regionid") != null && reader.GetAttribute("regionid") != string.Empty)
                                    region.RegionID = reader.GetAttribute("regionid");

                                if (reader.GetAttribute("NULLTEXT") != null && reader.GetAttribute("NULLTEXT") != string.Empty)
                                    region.NullText = reader.GetAttribute("NULLTEXT");
                                else if (reader.GetAttribute("nulltext") != null && reader.GetAttribute("nulltext") != string.Empty)
                                    region.NullText = reader.GetAttribute("nulltext");

                                if (reader.GetAttribute("URL") != null && reader.GetAttribute("URL") != string.Empty)
                                    region.URL = reader.GetAttribute("URL");
                                else if (reader.GetAttribute("url") != null && reader.GetAttribute("url") != string.Empty)
                                    region.URL = reader.GetAttribute("url");
                                Regions.Add(region);
                                break;
                        }
                        break;
                    case XmlNodeType.Text:
                        switch (element.ToUpper())
                        {
                            case "SUBJECT":
                                Message.Subject += reader.Value;
                                break;
                            case "BODYHTML":
                                Message.BodyHtml.Text += reader.Value;
                                break;
                            case "BODYTEXT":
                                Message.BodyText.Text += reader.Value;
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        element = string.Empty;
                        break;
                }
            }
        }
        /*/// <summary>
        /// Extract the value part of the specified text template line.
        /// </summary>
        /// <param name="line">The text template line.</param>
        /// <returns>The value part of the line.</returns>
        private string ExtractValue(string line)
        {
            ActiveUp.Net.Mail.Logger.AddEntry("Extracting property value from line: + " + line + " (raw).", 0);
            string propertyValue = line.Trim().Substring(line.IndexOf(":") + 1, line.Length - line.IndexOf(":") - 1).Trim();
            ActiveUp.Net.Mail.Logger.AddEntry("Extracted value: + " + propertyValue + ".", 0);
            return propertyValue != null ? propertyValue : string.Empty;
        }*/

        /// <summary>
        /// Load the content of the specified file.
        /// </summary>
        /// <param name="filename">The full path to the file</param>
        /// <returns>The content of the file</returns>
        private string LoadFileContent(string filename)
        {
            string content = string.Empty;
            
            if (filename.ToUpper().StartsWith("HTTP://") || filename.ToUpper().StartsWith("HTTPS://"))
            {
                WebRequest webRequest = WebRequest.Create(filename);
                WebResponse webResponse = webRequest.GetResponse();

                Stream stream = webResponse.GetResponseStream();
                content = new StreamReader(stream).ReadToEnd();
            }
            else
            {
                if (filename.ToUpper().StartsWith("FILE://"))
                    filename = filename.Substring(7);

                if (File.Exists(filename))
                {
                    TextReader textFileReader = TextReader.Synchronized(new StreamReader(filename, Encoding.ASCII));
                    content = textFileReader.ReadToEnd();
                    textFileReader.Close();
                }
                else
                {
                    throw new Exception("The specified file doesn't exist.");
                }
            }

            return content;
        }

        /// <summary>
        /// Gets or sets the region collection.
        /// </summary>
        public RegionCollection Regions
        {
            get { return _regions ?? (_regions = new RegionCollection()); }
            set { _regions = value; }
        }

        /// <summary>
        /// Gets or sets the conditional collection.
        /// </summary>
        public ConditionalCollection Conditions
        {
            get { return _conditions ?? (_conditions = new ConditionalCollection()); }
            set { _conditions = value; }
        }

        /// <summary>
        /// Gets or sets the fields format options.
        /// </summary>
        public FieldFormatCollection FieldsFormats
        {
            get { return _fieldsFormats ?? (_fieldsFormats = new FieldFormatCollection()); }
            set { _fieldsFormats = value; }
        }

        /*/// <summary>
        /// Gets or sets the message bodies.
        /// </summary>
        public ActiveUp.Net.Mail.BodyTemplateCollection Bodies
        {
            get
            {
                if (_bodies == null)
                    _bodies = new BodyTemplateCollection();
                return _bodies;
            }
            set
            {
                _bodies = value;
            }
        }*/

        /// <summary>
        /// Gets or sets the SMTP servers.
        /// </summary>
        public ServerCollection SmtpServers
        {
            get { return _smtpServers ?? (_smtpServers = new ServerCollection()); }
            set { _smtpServers = value; }
        }

        /// <summary>
        /// Gets or sets the List templates.
        /// </summary>
        public ListTemplateCollection ListTemplates
        {
            get { return _listTemplates ?? (_listTemplates = new ListTemplateCollection()); }
            set { _listTemplates = value; }
        }

        public void InitReplyTo()
        {
            if (Message.ReplyTo == null)
                Message.ReplyTo = new Address();
        }
    }
}
