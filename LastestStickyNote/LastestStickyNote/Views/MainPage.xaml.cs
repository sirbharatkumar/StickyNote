using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO.IsolatedStorage;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices.Automation;

namespace LastestStickyNote
{
    public partial class MainPage : UserControl
    {
        private static string UserId = string.Empty;
        public AddNotes addNote;
        public StickyNote stickyNote;
        public List<StickyNoteMaster> stickyNoteList;

        public MainPage()
        {
            InitializeComponent();

            //Check wheather the Application is running Out-Of-Browser.
            if (App.Current.IsRunningOutOfBrowser)
            {
                //If true then check for Permissions.
                if (Application.Current.HasElevatedPermissions)
                {
                    //Then maximize the Window size to full.
                    Application.Current.MainWindow.WindowState = WindowState.Maximized;
                }

            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    XDocument doc;
                    IsolatedStorageFileStream isoStream;

                    if (!isoStore.DirectoryExists("StickyNotes"))
                        isoStore.CreateDirectory("StickyNotes");

                    if (isoStore.FileExists(@"\StickyNotes\credentials.txt"))
                    {
                        chkAuthenticate.Visibility = Visibility.Collapsed;
                        chkDisableAuthenticate.Visibility = Visibility.Visible;
                        stackPassword.Visibility = Visibility.Collapsed;
                    }
                    if (isoStore.FileExists(@"\StickyNotes\Config.txt"))
                    {
                        isoStream = new IsolatedStorageFileStream(@"\StickyNotes\Config.txt", FileMode.Open, FileAccess.Read, isoStore);
                        TextReader reader = new StreamReader(isoStream);
                        string sLine = reader.ReadToEnd();
                        string[] strArray = sLine.Split('~');
                        isoStream.Close();
                        if (Convert.ToBoolean(strArray[1]))
                        {
                            ImageBrush imgBrush = new ImageBrush();
                            imgBrush.ImageSource = new BitmapImage(new Uri(strArray[0], UriKind.Relative));
                            MainGrid.Background = imgBrush;
                        }
                        else
                        {
                            ImageBrush imgBrush = new ImageBrush();
                            BitmapImage imageSource = new BitmapImage();
                            imgBrush.ImageSource = imageSource;

                            isoStream = new IsolatedStorageFileStream(strArray[0], FileMode.Open, isoStore);
                            imageSource.SetSource(isoStream);
                            isoStream.Close();
                            MainGrid.Background = imgBrush;
                        }
                    }
                    else
                    {
                        ImageBrush imgBrush = new ImageBrush();
                        imgBrush.ImageSource = new BitmapImage(new Uri(@"/LastestStickyNote;component/Images/HD Image 1.jpg", UriKind.Relative));
                        MainGrid.Background = imgBrush;
                    }

                    if (!isoStore.FileExists(@"\StickyNotes\StickyNotes.xml"))
                    {
                        isoStream = new IsolatedStorageFileStream(@"\StickyNotes\StickyNotes.xml", FileMode.Create, isoStore);

                        UserId = Guid.NewGuid().ToString();
                        doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
                                new XElement("stickynote",
                                    new XElement("user",
                                        new XAttribute("id", UserId),
                                        new XAttribute("enabledEncryption", false),
                                            new XElement("note",
                                                new XAttribute("id", 1),
                                                new XAttribute("header", "Default Note !!!"),
                                                new XAttribute("xLocation", 0),
                                                new XAttribute("yLocation", 0),
                                                new XAttribute("height", 250),
                                                new XAttribute("width", 250),
                                                new XAttribute("backgroundColor", "#FFFFFF"),
                                                new XCData("This is default note and can be deleted....")))));

                        doc.Save(isoStream);
                        isoStream.Close();

                        stickyNoteList = new List<StickyNoteMaster>();
                        stickyNoteList.Add(new StickyNoteMaster { Header = "Default Note !!!", Height = 250, Id = 1, Note = "This is default note and can be deleted....", Width = 250, XLocation = 0, YLocation = 0, BackgroundColor = "#FFFFFF" });

                        stickyNote = new StickyNote(UserId, "Default Note !!!", "This is default note and can be deleted....", new Thickness(0, 0, 0, 0), "1", 250, 250, "#FFFFFF");
                        stickyNote.ShowDialog();
                        stickyNote.AddNewClicked += new EventHandler(stickyNote_AddNewClicked);
                        LayoutRoot.Children.Add(stickyNote);
                    }
                    else
                    {
                        isoStream = new IsolatedStorageFileStream(@"\StickyNotes\StickyNotes.xml", FileMode.Open, isoStore);
                        doc = XDocument.Load(isoStream);

                        isoStream.Close();
                        User user = FileUtils.GetNotes(doc.Element("stickynote"));
                        UserId = user.UserId;
                        stickyNoteList = new List<StickyNoteMaster>();
                        //user.StickyNoteList.ToList().Reverse();
                        stickyNoteList = user.StickyNoteList.ToList();
                        foreach (StickyNoteMaster stickyNotes in user.StickyNoteList)
                        {
                            stickyNote = new StickyNote(UserId, stickyNotes.Header, stickyNotes.Note, new Thickness(stickyNotes.XLocation, stickyNotes.YLocation, 0, 0), stickyNotes.Id.ToString(), stickyNotes.Width, stickyNotes.Height, stickyNotes.BackgroundColor);
                            stickyNote.ShowDialog();
                            stickyNote.AddNewClicked += new EventHandler(stickyNote_AddNewClicked);
                            LayoutRoot.Children.Add(stickyNote);
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error while loading sticky note application.", "Error", MessageBoxButton.OK);
            }
        }

        void stickyNote_AddNewClicked(object sender, EventArgs e)
        {
            addNote = new AddNotes();
            addNote.AddClicked += new EventHandler(addNote_AddClicked);
            addNote.Show();
        }

        void addNote_AddClicked(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(addNote._header) && !string.IsNullOrEmpty(addNote._note))
                {
                    int maxId = stickyNoteList.Max(c => c.Id) + 1;

                    StickyNoteMaster master = new StickyNoteMaster();
                    master.Id = maxId;
                    master.Header = addNote._header;
                    master.Note = addNote._note;
                    master.Height = 250;
                    master.Width = 250;
                    master.XLocation = 0;
                    master.YLocation = 0;
                    master.BackgroundColor = "#FFFFFF";

                    User user = new User();
                    user.UserId = UserId;
                    user.StickyNoteList = new List<StickyNoteMaster>();
                    user.StickyNoteList.Add(master);

                    if (FileUtils.ProcessStickyNote(user, FileOperation.AddNote))
                    {
                        stickyNoteList.Add(new StickyNoteMaster { Header = addNote._header, Height = 250, Id = maxId, Note = addNote._note, Width = 250, XLocation = 0, YLocation = 0, BackgroundColor = "#FFFFFF" });
                        stickyNote = new StickyNote(UserId, addNote._header, addNote._note, new Thickness(0, 0, 0, 0), maxId.ToString(), 250, 250, "#FFFFFF");
                        stickyNote.ShowDialog();
                        stickyNote.AddNewClicked += new EventHandler(stickyNote_AddNewClicked);
                        LayoutRoot.Children.Add(stickyNote);
                    }
                }
                else
                {
                    MessageBox.Show("*Header and Note are required.", "Error", MessageBoxButton.OK);
                    addNote = new AddNotes();
                    addNote.AddClicked += new EventHandler(addNote_AddClicked);
                    addNote.Show();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

        }

        private void btnSetPassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtUserName.Text) && !string.IsNullOrEmpty(txtPassword.Password))
                {
                    using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        XDocument doc;
                        IsolatedStorageFileStream isoStream;

                        if (!isoStore.DirectoryExists("StickyNotes"))
                            isoStore.CreateDirectory("StickyNotes");

                        //if the records file does not exist
                        if (isoStore.FileExists(@"\StickyNotes\credentials.txt"))
                            isoStore.DeleteFile(@"\StickyNotes\credentials.txt");

                        //create new document
                        isoStream = new IsolatedStorageFileStream(@"\StickyNotes\credentials.txt", FileMode.Create, isoStore);

                        doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
                                    new XElement("credentials",
                                        new XElement("credential",
                                            new XAttribute("username", txtUserName.Text), new XAttribute("password", txtPassword.Password), new XAttribute("timecreated", DateTime.Now.ToString()))));

                        byte[] byteData = Encoding.UTF8.GetBytes(doc.ToString());
                        TextWriter writer = new StreamWriter(isoStream);
                        writer.WriteLine(Convert.ToBase64String(byteData));
                        writer.Close(); // Close the writer so data is flushed
                        isoStream.Close(); // Close the stream too               
                    }
                    System.Windows.MessageBox.Show("*Please close the application and reopen it.", "Information", MessageBoxButton.OK);
                    stackPassword.Visibility = Visibility.Collapsed;
                    chkAuthenticate.Visibility = Visibility.Collapsed;
                    chkDisableAuthenticate.Visibility = Visibility.Visible;
                    chkDisableAuthenticate.IsChecked = false;
                }
                else
                {
                    System.Windows.MessageBox.Show("*Name and Password required.", "Information", MessageBoxButton.OK);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error while setting up the authentication.", "Error", MessageBoxButton.OK);
            }
        }

        private void chkAuthenticate_Checked(object sender, RoutedEventArgs e)
        {
            stackPassword.Visibility = Visibility.Visible;
        }

        private void chkAuthenticate_Unchecked(object sender, RoutedEventArgs e)
        {
            stackPassword.Visibility = Visibility.Collapsed;
        }

        private void closeWindow_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (App.Current.IsRunningOutOfBrowser)
                Application.Current.MainWindow.Close();
        }

        private void minimizeWindow_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void chkDisableAuthenticate_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    //if the records file does not exist
                    if (isoStore.FileExists(@"\StickyNotes\credentials.txt"))
                        isoStore.DeleteFile(@"\StickyNotes\credentials.txt");


                    chkAuthenticate.Visibility = Visibility.Visible;
                    chkAuthenticate.IsChecked = false;
                    chkDisableAuthenticate.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error while disabling the authentication.", "Error", MessageBoxButton.OK);
            }
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    string imagePath = (sender as System.Windows.Controls.Image).Tag.ToString();

                    IsolatedStorageFileStream isoStream;

                    if (isoStore.FileExists(@"\StickyNotes\Config.txt"))
                        isoStore.DeleteFile(@"\StickyNotes\Config.txt");

                    //create new document
                    isoStream = new IsolatedStorageFileStream(@"\StickyNotes\Config.txt", FileMode.Create, isoStore);

                    TextWriter writer = new StreamWriter(isoStream);
                    writer.WriteLine(imagePath + "~true");
                    writer.Close(); // Close the writer so data is flushed
                    isoStream.Close(); // Close the stream too 

                    ImageBrush imgBrush = new ImageBrush();
                    imgBrush.ImageSource = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                    MainGrid.Background = imgBrush;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error while setting up the background image.", "Error", MessageBoxButton.OK);
            }

        }

        private void imgUpload_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            bool? result = ofd.ShowDialog();
            if (!result.HasValue || result.Value == false)
                return;

            ImageBrush imgBrush = new ImageBrush();
            BitmapImage imageSource = new BitmapImage();
            imgBrush.ImageSource = imageSource;
            try
            {
                string ext = ofd.File.Extension;
                FileStream stream = ofd.File.OpenRead();

                if (stream.Length < 1000000)
                {
                    using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                    {

                        IsolatedStorageFileStream isoStream;

                        if (!isoStore.DirectoryExists("BackgroundImage"))
                            isoStore.CreateDirectory("BackgroundImage");

                        string FolderName = "\\BackgroundImage\\*";
                        string[] files = isoStore.GetFileNames(FolderName);
                        string path = string.Empty;
                        foreach (string fileName in files)
                        {
                            path = @"\BackgroundImage\" + fileName;
                            if (isoStore.FileExists(path))
                            {
                                isoStore.DeleteFile(@"\BackgroundImage\" + fileName);
                            }
                        }

                        if (isoStore.FileExists(@"\StickyNotes\Config.txt"))
                            isoStore.DeleteFile(@"\StickyNotes\Config.txt");

                        //create new document
                        isoStream = new IsolatedStorageFileStream(@"\StickyNotes\Config.txt", FileMode.Create, isoStore);

                        TextWriter writer = new StreamWriter(isoStream);
                        writer.WriteLine(@"\BackgroundImage\BackgroundImage" + ofd.File.Extension + "~false");
                        writer.Close(); // Close the writer so data is flushed
                        isoStream.Close(); // Close the stream too 


                        isoStream = new IsolatedStorageFileStream(@"\BackgroundImage\BackgroundImage" + ofd.File.Extension, FileMode.Create, isoStore);

                        //// Read and write the data block by block until finish
                        while (true)
                        {
                            byte[] buffer = new byte[stream.Length];
                            int count = stream.Read(buffer, 0, buffer.Length);
                            if (count > 0)
                            {
                                isoStream.Write(buffer, 0, count);
                            }
                            else
                            {
                                break;
                            }
                        }
                        isoStream.Close();

                        imageSource.SetSource(ofd.File.OpenRead());
                        MainGrid.Background = imgBrush;
                    }
                }
                else
                {
                    MessageBox.Show("Background image should be less than 1MB.", "Error", MessageBoxButton.OK);
                }

            }
            catch (Exception)
            {
                MessageBox.Show("Error while setting up the background image.", "Error", MessageBoxButton.OK);
            }

        }

        private void MainGrid_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
