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

namespace Liquid
{
    #region Public Enums

    public enum MoreCursors
    {
        None,
        SizeNWSE,
        SizeNESW,
        Resize
    };

    #endregion

    /// <summary>
    /// Allows the display of non-standard cursors
    /// </summary>
    public partial class CursorPlus : Control
    {
        #region Visual Elements

        /// <summary> 
        /// Root element template.
        /// </summary>
        public FrameworkElement ElementRoot { get; set; }
        internal const string ElementRootName = "RootElement";

        /// <summary> 
        /// Top-Left - Bottom-Right template.
        /// </summary>
        internal Polygon ElementNWSE { get; set; }
        internal const string ElementNWSEName = "ElementNWSE";

        /// <summary> 
        /// Top-Right - Bottom-Left template.
        /// </summary>
        internal Polygon ElementNESW { get; set; }
        internal const string ElementNESWName = "ElementNESW";

        /// <summary> 
        /// ElementResize template.
        /// </summary>
        internal Polygon ElementResize { get; set; }
        internal const string ElementResizeName = "ElementResize";

        #endregion

        #region Private Properties

        private MoreCursors _type = MoreCursors.None;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the type of cursor to display
        /// </summary>
        public MoreCursors Type
        {
            get { return _type; }
            set { _type = value; UpdateVisualState(); }
        }

        #endregion

        #region Constructor

        public CursorPlus()
        {
            DefaultStyleKey = this.GetType();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Displays the cursor at the provided position
        /// </summary>
        /// <param name="type">Cursor type</param>
        /// <param name="position">Position to render</param>
        public void Show(MoreCursors type, Point position)
        {
            this.SetValue(Canvas.LeftProperty, position.X - (double.IsNaN(Width) ? 0 : Width * 0.5));
            this.SetValue(Canvas.TopProperty, position.Y - (double.IsNaN(Height) ? 0 : Height * 0.5));

            Type = type;
        }

        /// <summary>
        /// Hides the cursor
        /// </summary>
        public void Hide()
        {
            Type = MoreCursors.None;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the visual state of the control
        /// </summary>
        protected void UpdateVisualState()
        {
            if (_type == MoreCursors.None)
            {
                this.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.Visibility = Visibility.Visible;
            }

            if (ElementRoot != null)
            {
                ElementNWSE.Visibility = Visibility.Collapsed;
                ElementNESW.Visibility = Visibility.Collapsed;
                ElementResize.Visibility = Visibility.Collapsed;
                Width = 16;
                Height = 16;
                ElementRoot.Width = Width;
                ElementRoot.Height = Height;

                switch (_type)
                {
                    case MoreCursors.SizeNESW:
                        ElementNESW.Visibility = Visibility.Visible;
                        break;
                    case MoreCursors.SizeNWSE:
                        ElementNWSE.Visibility = Visibility.Visible;
                        break;
                    case MoreCursors.Resize:
                        ElementResize.Visibility = Visibility.Visible;
                        Width = 22;
                        Height = 22;
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        #region Event Handling

        /// <summary>
        /// This is called when the template has been bound to the control
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ElementRoot = (FrameworkElement)GetTemplateChild(ElementRootName);
            ElementNWSE = (Polygon)GetTemplateChild(ElementNWSEName);
            ElementNESW = (Polygon)GetTemplateChild(ElementNESWName);
            ElementResize = (Polygon)GetTemplateChild(ElementResizeName);

            UpdateVisualState();
        }

        #endregion
    }
}
