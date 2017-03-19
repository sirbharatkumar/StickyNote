using System;
using System.Linq;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Collections.Generic;

namespace LastestStickyNote
{
    public class FileUtils
    {
        public static bool ProcessStickyNote(User user, FileOperation operation)
        {
            try
            {
                switch (operation)
                {
                    case FileOperation.UpdateNote:
                        using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            IsolatedStorageFileStream isoStream;

                            isoStream = new IsolatedStorageFileStream(@"\StickyNotes\StickyNotes.xml", FileMode.Open, isoStore);

                            XElement rootElement = XElement.Load(isoStream);
                            isoStream.Close();

                            string xPath = string.Format("/*[@id='{0}']/*[@id='{1}']", user.UserId, user.StickyNoteList[0].Id);

                            // Select the element
                            XElement itemElement = rootElement.XPathSelectElement(xPath);

                            // XElement newElement;
                            if (itemElement != null)
                            {
                                XElement newElement = new XElement("note",
                                                        new XAttribute("id", user.StickyNoteList[0].Id),
                                                        new XAttribute("header", user.StickyNoteList[0].Header),
                                                        new XAttribute("xLocation", itemElement.Attribute("xLocation").Value),
                                                        new XAttribute("yLocation", itemElement.Attribute("yLocation").Value),
                                                        new XAttribute("height", itemElement.Attribute("height").Value),
                                                        new XAttribute("width", itemElement.Attribute("width").Value),
                                                        new XAttribute("backgroundColor", itemElement.Attribute("backgroundColor").Value),
                                                        new XCData(user.StickyNoteList[0].Note));

                                itemElement.Add(newElement);

                                itemElement.ReplaceWith(newElement);
                            }
                            isoStore.DeleteFile(@"\StickyNotes\StickyNotes.xml");
                            isoStream.Close();

                            isoStream = new IsolatedStorageFileStream(@"\StickyNotes\StickyNotes.xml", FileMode.Create, isoStore);

                            rootElement.Save(isoStream);
                            isoStream.Close();
                        }
                        break;
                    case FileOperation.UpdatePosition:
                        using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            IsolatedStorageFileStream isoStream;

                            isoStream = new IsolatedStorageFileStream(@"\StickyNotes\StickyNotes.xml", FileMode.Open, isoStore);

                            XElement rootElement = XElement.Load(isoStream);
                            isoStream.Close();

                            string xPath = string.Format("/*[@id='{0}']/*[@id='{1}']", user.UserId, user.StickyNoteList[0].Id);

                            // Select the element
                            XElement itemElement = rootElement.XPathSelectElement(xPath);

                            // XElement newElement;
                            if (itemElement != null)
                            {
                                XElement newElement = new XElement("note",
                                                        new XAttribute("id", user.StickyNoteList[0].Id),
                                                        new XAttribute("header", user.StickyNoteList[0].Header),
                                                        new XAttribute("xLocation", user.StickyNoteList[0].XLocation),
                                                        new XAttribute("yLocation", user.StickyNoteList[0].YLocation),
                                                        new XAttribute("height", user.StickyNoteList[0].Height),
                                                        new XAttribute("width", user.StickyNoteList[0].Width),
                                                        new XAttribute("backgroundColor", itemElement.Attribute("backgroundColor").Value),
                                                        new XCData(user.StickyNoteList[0].Note));

                                itemElement.Add(newElement);

                                itemElement.ReplaceWith(newElement);
                            }
                            isoStore.DeleteFile(@"\StickyNotes\StickyNotes.xml");
                            isoStream.Close();

                            isoStream = new IsolatedStorageFileStream(@"\StickyNotes\StickyNotes.xml", FileMode.Create, isoStore);

                            rootElement.Save(isoStream);
                            isoStream.Close();
                        }
                        break;
                    case FileOperation.UpdateBackgroundColor:
                        using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            IsolatedStorageFileStream isoStream;

                            isoStream = new IsolatedStorageFileStream(@"\StickyNotes\StickyNotes.xml", FileMode.Open, isoStore);

                            XElement rootElement = XElement.Load(isoStream);
                            isoStream.Close();

                            string xPath = string.Format("/*[@id='{0}']/*[@id='{1}']", user.UserId, user.StickyNoteList[0].Id);

                            // Select the element
                            XElement itemElement = rootElement.XPathSelectElement(xPath);

                            // XElement newElement;
                            if (itemElement != null)
                            {
                                XElement newElement = new XElement("note",
                                                        new XAttribute("id", itemElement.Attribute("id").Value),
                                                        new XAttribute("header", itemElement.Attribute("header").Value),
                                                        new XAttribute("xLocation", itemElement.Attribute("xLocation").Value),
                                                        new XAttribute("yLocation", itemElement.Attribute("yLocation").Value),
                                                        new XAttribute("height", itemElement.Attribute("height").Value),
                                                        new XAttribute("width", itemElement.Attribute("width").Value),
                                                        new XAttribute("backgroundColor", user.StickyNoteList[0].BackgroundColor),
                                                        new XCData(itemElement.Value));

                                itemElement.Add(newElement);

                                itemElement.ReplaceWith(newElement);
                            }
                            isoStore.DeleteFile(@"\StickyNotes\StickyNotes.xml");
                            isoStream.Close();

                            isoStream = new IsolatedStorageFileStream(@"\StickyNotes\StickyNotes.xml", FileMode.Create, isoStore);

                            rootElement.Save(isoStream);
                            isoStream.Close();
                        }
                        break;
                    case FileOperation.DeleteNote:
                        using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            IsolatedStorageFileStream isoStream;

                            isoStream = new IsolatedStorageFileStream(@"\StickyNotes\StickyNotes.xml", FileMode.Open, isoStore);

                            XElement rootElement = XElement.Load(isoStream);
                            isoStream.Close();


                            User userNote = FileUtils.GetNotes(rootElement);
                            int stickyNoteCount = userNote.StickyNoteList.ToList().Count;

                            //int countStickyNote = FileUtils.GetNotes(rootElement.Element("stickynote")).StickyNoteList.Count;

                            if (stickyNoteCount > 1)
                            {
                                string xPath = string.Format("/*[@id='{0}']/*[@id='{1}']", user.UserId, user.StickyNoteList[0].Id);

                                // Select the element
                                XElement itemElement = rootElement.XPathSelectElement(xPath);

                                // XElement newElement;
                                if (itemElement != null)
                                {
                                    itemElement.Remove();
                                }

                                isoStore.DeleteFile(@"\StickyNotes\StickyNotes.xml");
                                isoStream.Close();

                                isoStream = new IsolatedStorageFileStream(@"\StickyNotes\StickyNotes.xml", FileMode.Create, isoStore);

                                rootElement.Save(isoStream);
                                isoStream.Close();
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("*Can't delete the last note, there should be atleast one note.", "Information", System.Windows.MessageBoxButton.OK);
                                return false;
                            }
                        }
                        break;
                    case FileOperation.AddNote:
                        using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            IsolatedStorageFileStream isoStream;

                            isoStream = new IsolatedStorageFileStream(@"\StickyNotes\StickyNotes.xml", FileMode.Open, isoStore);

                            XElement rootElement = XElement.Load(isoStream);
                            isoStream.Close();

                            string xPath = string.Format("/*[@id='{0}']", user.UserId);

                            // Select the element
                            XElement itemElement = rootElement.XPathSelectElement(xPath);

                            //XElement newElement;
                            if (itemElement != null)
                            {
                                XElement newElement = new XElement("note",
                                                        new XAttribute("id", user.StickyNoteList[0].Id),
                                                        new XAttribute("header", user.StickyNoteList[0].Header),
                                                        new XAttribute("xLocation", user.StickyNoteList[0].XLocation),
                                                        new XAttribute("yLocation", user.StickyNoteList[0].YLocation),
                                                        new XAttribute("height", user.StickyNoteList[0].Height),
                                                        new XAttribute("width", user.StickyNoteList[0].Width),
                                                        new XAttribute("backgroundColor", user.StickyNoteList[0].BackgroundColor),
                                                        new XCData(user.StickyNoteList[0].Note));
                                itemElement.Add(newElement);
                            }
                            isoStore.DeleteFile(@"\StickyNotes\StickyNotes.xml");
                            isoStream.Close();

                            isoStream = new IsolatedStorageFileStream(@"\StickyNotes\StickyNotes.xml", FileMode.Create, isoStore);

                            rootElement.Save(isoStream);
                            isoStream.Close();
                        }
                        break;
                    default:
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return false;
            }
        }

        public static User GetNotes(XElement element)
        {
            User user = new User();
            user.UserId = element.Element("user").Attribute("id").Value;
            user.DataEncrypted = false;
            List<StickyNoteMaster> stickyNoteMaster = (from notes in element.Element("user").Elements("note")
                                                       select new StickyNoteMaster()
                                                       {
                                                           Id = Convert.ToInt32(notes.Attribute("id").Value),
                                                           Header = notes.Attribute("header").Value,
                                                           BackgroundColor = notes.Attribute("backgroundColor").Value,
                                                           XLocation = Convert.ToInt32(notes.Attribute("xLocation").Value),
                                                           YLocation = Convert.ToInt32(notes.Attribute("yLocation").Value),
                                                           Width = Convert.ToInt32(notes.Attribute("width").Value),
                                                           Height = Convert.ToInt32(notes.Attribute("height").Value),
                                                           Note = notes.Value
                                                       }).ToList();

            user.StickyNoteList = stickyNoteMaster;
            return user;
        }
    }

    public enum FileOperation
    {
        UpdateNote,
        UpdatePosition,
        UpdateBackgroundColor,
        DeleteNote,
        AddNote
    }
}
