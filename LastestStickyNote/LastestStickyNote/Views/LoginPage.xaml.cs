using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace LastestStickyNote
{
    public partial class LoginPage : UserControl
    {
        public LoginPage()
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
                    IsolatedStorageFileStream isoStream;

                    if (!isoStore.DirectoryExists("StickyNotes"))
                        isoStore.CreateDirectory("StickyNotes");

                    //if the records file does not exist
                    if (!isoStore.FileExists(@"\StickyNotes\credentials.txt"))
                    {
                        this.Content = new MainPage();
                    }

                    else
                    {
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
                                LayoutRoot.Background = imgBrush;
                            }
                            else
                            {
                                ImageBrush imgBrush = new ImageBrush();
                                BitmapImage imageSource = new BitmapImage();
                                imgBrush.ImageSource = imageSource;

                                isoStream = new IsolatedStorageFileStream(strArray[0], FileMode.Open, isoStore);
                                imageSource.SetSource(isoStream);
                                isoStream.Close();
                                LayoutRoot.Background = imgBrush;
                            }
                        }
                        else
                        {
                            ImageBrush imgBrush = new ImageBrush();
                            imgBrush.ImageSource = new BitmapImage(new Uri(@"/LastestStickyNote;component/Images/HD Image 1.jpg", UriKind.Relative));
                            LayoutRoot.Background = imgBrush;
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error while loading sticky note application.", "Error", MessageBoxButton.OK);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (App.Current.IsRunningOutOfBrowser)
                App.Current.MainWindow.Close();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    XElement doc;
                    IsolatedStorageFileStream isoStream;

                    //if the records file does not exist
                    if (isoStore.FileExists(@"\StickyNotes\credentials.txt"))
                    {
                        isoStream = new IsolatedStorageFileStream(@"\StickyNotes\credentials.txt", FileMode.Open, FileAccess.Read, isoStore);
                        TextReader reader = new StreamReader(isoStream);
                        string sLine = reader.ReadToEnd();
                        isoStream.Close();
                        byte[] reverse = Convert.FromBase64String(sLine);
                        string reverseDoc = Encoding.UTF8.GetString(reverse, 0, reverse.Length);
                        doc = XElement.Parse(reverseDoc);

                        string userName = doc.Element("credential").Attribute("username").Value;
                        string password = doc.Element("credential").Attribute("password").Value;
                        if (txtUserName.Text == userName && txtPassword.Password == password)
                        {
                            this.Content = new MainPage();
                        }
                        else
                        {
                            MessageBox.Show("Credentials didn't matched...", "Error", MessageBoxButton.OK);
                        }
                    }                    
                }

            }
            catch
            {
                MessageBox.Show("Error while login to the system, please try after sometime...", "Error", MessageBoxButton.OK);
            }
        }
    }
}
