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

namespace Liquid
{
    public partial class TimePickerPlus : LiquidControl
    {
        #region Visual Elements

		/// <summary> 
		/// Hours template.
		/// </summary>
		internal ComboBox ElementHours { get; set; }
		internal const string ElementHoursName = "ElementHours";

		/// <summary> 
		/// Minutes template.
		/// </summary>
		internal ComboBox ElementMinutes { get; set; }
		internal const string ElementMinutesName = "ElementMinutes";

		#endregion

		#region Private Properties

		private DateTime _value = DateTime.Now;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the value of the control
		/// </summary>
		public DateTime Value
		{
			get { return _value; }
			set
			{
				_value = value;
				if (ElementRoot != null)
				{
					ElementHours.SelectedItem = _value.Hour.ToString().PadLeft(2, '0');
                    ElementMinutes.SelectedItem = _value.Minute.ToString().PadLeft(2, '0');
				}

                RaiseChanged(this, EventArgs.Empty);
			}
		}


		#endregion

		#region Public Events

		public event EventHandler ValueChanged;

		#endregion

		#region Constructor

        public TimePickerPlus()
		{
			IsTabStop = true;
			TabNavigation = KeyboardNavigationMode.Once;
		}

		#endregion

		#region Public Methods

		#endregion

		#region Private Methods

		protected override void UpdateVisualState()
		{
			base.UpdateVisualState();
		}

		#endregion

		#region Event Handling

		/// <summary>
		/// This is called when the template has been bound to the control
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

            ElementHours = (ComboBox)GetTemplateChild(ElementHoursName);
            ElementMinutes = (ComboBox)GetTemplateChild(ElementMinutesName);

            ElementHours.SelectionChanged += new SelectionChangedEventHandler(ComboBox_SelectionChanged);
            ElementMinutes.SelectionChanged += new SelectionChangedEventHandler(ComboBox_SelectionChanged);

            int i;

            for (i = 0; i <= 23; i++)
            {
                ElementHours.Items.Add(i.ToString().PadLeft(2, '0'));
            }

            for (i = 0; i <= 59; i++)
            {
                ElementMinutes.Items.Add(i.ToString().PadLeft(2, '0'));
            }

			UpdateVisualState();

            ElementHours.SelectedItem = _value.Hour.ToString().PadLeft(2, '0');
            ElementMinutes.SelectedItem = _value.Minute.ToString().PadLeft(2, '0');
		}

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ElementHours.SelectedItem != null && ElementMinutes.SelectedItem != null)
            {
                _value = new DateTime(_value.Year, _value.Month, _value.Day, int.Parse(ElementHours.SelectedItem.ToString()), int.Parse(ElementMinutes.SelectedItem.ToString()), 0);
            }
        }

		#endregion

		#region Event Raising

		/// <summary>
		/// Generates a Clicked event to indicate the checkbox has been clicked
		/// </summary>
		/// <param name="sender">Event source object</param>
		/// <param name="args">Event arguments</param>
		protected virtual void RaiseChanged(object sender, EventArgs args)
		{
            if (ValueChanged != null)
			{
                ValueChanged(sender, args);
			}
		}

		#endregion
    }
}
