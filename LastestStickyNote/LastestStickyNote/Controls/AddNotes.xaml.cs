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

namespace LastestStickyNote
{
    public partial class AddNotes : ChildWindow
    {
        /// <summary>
        /// Delegate AddClicked.
        /// </summary>
        public event EventHandler AddClicked;

        /// <summary>
        /// Global properties.
        /// </summary>
        public string _header { get; set; }
        public string _note { get; set; }


        /// <summary>
        /// Initialize AddNote 
        /// </summary>
        public AddNotes()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Calls the delegate to add the header and note to XML and add sticky note to Grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddClicked != null)
            {
                _header = txtHeader.Text;
                _note = txtNote.Text;
                AddClicked(this, new EventArgs());
            }
            this.DialogResult = false;
        }

        /// <summary>
        /// Close the dialog without saving.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

