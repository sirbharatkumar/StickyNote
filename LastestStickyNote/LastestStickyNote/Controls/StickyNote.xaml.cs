using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices.Automation;

namespace LastestStickyNote
{
    public partial class StickyNote : UserControl
    {
        /// <summary>
        /// Global properties
        /// </summary>
        public string StickyNoteText { get; set; }
        public string StickyNoteHeader { get; set; }
        public string UserId { get; set; }
        public Thickness PopupPosition { get; set; }

        /// <summary>
        /// Delegate AddNewClicked.
        /// </summary>
        public event EventHandler AddNewClicked;

        /// <summary>
        /// Initialize StickyNote
        /// </summary>
        public StickyNote()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Initialize StickyNote with sticky note properties
        /// </summary>
        /// <param name="userId">string UserId</param>
        /// <param name="stickyNoteHeader">string StickyNoteHeader</param>
        /// <param name="stickyNoteText">string StickyNoteText</param>
        /// <param name="position">Thickness Position</param>
        /// <param name="tag">string Tag</param>
        /// <param name="width">double Width</param>
        /// <param name="height">double Height</param>
        /// <param name="backgroundColor">string BackgroundColor</param>
        public StickyNote(string userId, string stickyNoteHeader, string stickyNoteText, Thickness position, string tag, double width, double height,string backgroundColor)
        {
            try
            {
                InitializeComponent();
                StickyNoteHeader = stickyNoteHeader;
                StickyNoteText = stickyNoteText;
                UserId = userId;
                PopupPosition = position;
                stickyNote.Tag = txtStickyNote.Tag = tag;                
                txtStickyNote.Background = CombineAlphaAndColorInSolidColorBrush(1, backgroundColor);
                if (height < 30)
                {
                    stickyNote.Width = 250;
                    stickyNote.Height = 250;                   
                }
                else
                {
                    stickyNote.Height = height;
                    stickyNote.Width = width;
                }
                
                stickyNote.Closed += new Liquid.DialogEventHandler(stickyNote_Closed);
                txtStickyNote.TextChanged += new TextChangedEventHandler(txtStickyNote_TextChanged);

            }
            catch (Exception)
            {
                MessageBox.Show("Failed to open note..", "Error", MessageBoxButton.OK);
            }
        }

        void stickyNote_Closed(object sender, Liquid.DialogEventArgs e)
        {
            try
            {
                MessageBoxResult isConfirmed = MessageBox.Show("Are you sure you want to delete the note.", "Message", MessageBoxButton.OKCancel);

                if (isConfirmed == MessageBoxResult.OK)
                {

                    StickyNoteMaster master = new StickyNoteMaster();
                    master.Id = Convert.ToInt32(txtStickyNote.Tag.ToString());

                    User user = new User();
                    user.UserId = UserId;
                    user.StickyNoteList = new List<StickyNoteMaster>();
                    user.StickyNoteList.Add(master);

                    if (FileUtils.ProcessStickyNote(user, FileOperation.DeleteNote))
                    {
                        stickyNote.IsOpen = false;
                    }

                }
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to delete note..", "Error", MessageBoxButton.OK);
            }
        }

        void txtStickyNote_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                StickyNoteText = txtStickyNote.Text;

                StickyNoteMaster master = new StickyNoteMaster();
                master.Id = Convert.ToInt32(txtStickyNote.Tag.ToString());
                master.Header = StickyNoteHeader;
                master.Note = StickyNoteText;             

                User user = new User();
                user.UserId = UserId;
                user.StickyNoteList = new List<StickyNoteMaster>();
                user.StickyNoteList.Add(master);

                FileUtils.ProcessStickyNote(user, FileOperation.UpdateNote);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to update note..", "Error", MessageBoxButton.OK);
            }

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stickyNote.Title = StickyNoteHeader;
            txtStickyNote.Text = StickyNoteText;
            stickyNote.UseLayoutRounding = true;
            stickyNote.Margin = PopupPosition;           
        }

        private void stickyNote_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            txtStickyNote.Width = stickyNote.Width - 5;
            if (stickyNote.Height >= 30)
            {
                txtStickyNote.Height = stickyNote.Height - 55;
            }
            else
            {
                txtStickyNote.Height = stickyNote.Height - 5;
            }
        }

        public void ShowDialog()
        {
            stickyNote.Show();           
        }

        private void imgAddNote_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (AddNewClicked != null)
            {
                AddNewClicked(this, new EventArgs());
            }
        }

        private void stickyNote_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                GeneralTransform gt = stickyNote.TransformToVisual(Application.Current.RootVisual as UIElement);
                Point offset = gt.Transform(new Point(0, 0));
                double controlTop = offset.Y;
                double controlLeft = offset.X;

                StickyNoteMaster master = new StickyNoteMaster();
                master.Id = Convert.ToInt32(txtStickyNote.Tag.ToString());
                master.Header = StickyNoteHeader;
                master.Note = StickyNoteText;
                master.Height = stickyNote.Height;
                master.Width = stickyNote.Width;
                master.XLocation = Convert.ToInt32(controlLeft);
                master.YLocation = Convert.ToInt32(controlTop);

                User user = new User();
                user.UserId = UserId;
                user.StickyNoteList = new List<StickyNoteMaster>();
                user.StickyNoteList.Add(master);

                FileUtils.ProcessStickyNote(user, FileOperation.UpdatePosition);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to update note..", "Error", MessageBoxButton.OK);
            }
        }

        private void imgSend_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                using (dynamic outlook = AutomationFactory.CreateObject("Outlook.Application"))
                {
                    dynamic mail = outlook.CreateItem(0);
                    mail.Subject = this.StickyNoteHeader;
                    mail.HTMLBody = this.txtStickyNote.Text;
                    mail.Display();
                }
            }
            catch
            {
                MessageBox.Show("Failed to open outlook. Please check wheather outlook is configured.", "Error", MessageBoxButton.OK);
            }
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                StickyNoteMaster master = new StickyNoteMaster();
                master.Id = Convert.ToInt32(txtStickyNote.Tag.ToString());               
                master.BackgroundColor = (sender as StackPanel).Tag.ToString();

                User user = new User();
                user.UserId = UserId;
                user.StickyNoteList = new List<StickyNoteMaster>();
                user.StickyNoteList.Add(master);

                FileUtils.ProcessStickyNote(user, FileOperation.UpdateBackgroundColor);
                txtStickyNote.Background = CombineAlphaAndColorInSolidColorBrush(1, (sender as StackPanel).Tag.ToString());
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to update note..", "Error", MessageBoxButton.OK);
            }
            
        }

        #region protected static SolidColorBrush CombineAlphaAndColorInSolidColorBrush(double opacity, string color)
        /// <summary>
        /// adds the alpha (opacity) value to the front of the color string
        /// </summary>
        /// <param name="opacity"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        protected static SolidColorBrush CombineAlphaAndColorInSolidColorBrush(double opacity, string color)
        {
            SolidColorBrush theAnswer = new SolidColorBrush();

            // //////////////////////////////////////////////////////
            // deal with opacity

            if (opacity > 1.0)
                opacity = 1.0;

            if (opacity < 0.0)
                opacity = 0.0;

            // get the hex value of the alpha chanel (opacity):
            byte a = (byte)(Convert.ToInt32(255 * opacity));

            // --cmt:5afff4b1-daca-4daf-ab56-64864c80235e--
            // deal with the color

            try
            {
                byte r = (byte)(Convert.ToUInt32(color.Substring(1, 2), 16));
                byte g = (byte)(Convert.ToUInt32(color.Substring(3, 2), 16));
                byte b = (byte)(Convert.ToUInt32(color.Substring(5, 2), 16));

                theAnswer.Color = Color.FromArgb(a, r, g, b);
            }
            catch
            {
                // pick a fugly color, but don't cause the system to barf.
                theAnswer.Color = Color.FromArgb(255, 255, 0, 0);
            }

            return theAnswer;
        }
        #endregion

        private void stickyNote_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
               
    }
}
