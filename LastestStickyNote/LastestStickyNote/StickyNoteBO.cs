using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace LastestStickyNote
{   
  
    public class User
    {        
        public string UserId { get; set; }       
        public bool DataEncrypted { get; set; }        
        public List<StickyNoteMaster> StickyNoteList { get; set; }
    }
    
    public class StickyNoteMaster
    {      
        public string Header { get; set; }       
        public int Id { get; set; }      
        public int XLocation { get; set; }       
        public int YLocation { get; set; }       
        public double Height { get; set; }      
        public double Width { get; set; }       
        public string Note { get; set; }
        public string BackgroundColor { get; set; }
    }
}
