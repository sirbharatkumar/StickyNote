using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace Liquid
{
	public partial class NumericUpDownPlus : LiquidControl
	{
		#region Visual Elements

		/// <summary> 
		/// Textbox template.
		/// </summary>
		internal TextBox ElementInput { get; set; }
		internal const string ElementInputName = "ElementInput";

		/// <summary> 
		/// Up Button template.
		/// </summary>
		internal RepeatButton ElementUp { get; set; }
		internal const string ElementUpName = "ElementUp";

		/// <summary> 
		/// Down Button template.
		/// </summary>
		internal RepeatButton ElementDown { get; set; }
		internal const string ElementDownName = "ElementDown";

		#endregion

		#region Private Properties

		private int _value = 0;

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the value of the control
		/// </summary>
		public int Value
		{
			get { return _value; }
			set
			{
                int oldVal = _value;

				if (value < Min)
				{
					value = Min;
				}
				if (value > Max)
				{
					value = Max;
				}

				_value = value;
				if (ElementRoot != null)
				{
					ElementInput.Text = _value.ToString();
				}
                if (oldVal != _value)
                {
                    RaiseChanged(this, EventArgs.Empty);
                }
			}
		}

		/// <summary>
		/// Gets or sets the minimum acceptable value
		/// </summary>
		public int Min { get; set; }

		/// <summary>
		/// Gets or sets the maximum acceptable value
		/// </summary>
		public int Max { get; set; }

		/// <summary>
		/// Gets or sets the amount to increment/decrement by
		/// </summary>
		public int Step { get; set; }

		/// <summary>
		/// Gets or sets the amount of time in miliseconds between repeats
		/// </summary>
		public int Interval { get; set; }

		#endregion

		#region Public Events

		public event EventHandler ValueChanged;

		#endregion

		#region Constructor

        public NumericUpDownPlus()
		{
			Min = 0;
			Max = 32767;
			Step = 1;
			Interval = 50;

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

			ElementInput = (TextBox)GetTemplateChild(ElementInputName);
			ElementUp = (RepeatButton)GetTemplateChild(ElementUpName);
			ElementDown = (RepeatButton)GetTemplateChild(ElementDownName);

			ElementInput.TextChanged += new TextChangedEventHandler(ElementInput_TextChanged);
			ElementUp.Click += new RoutedEventHandler(ElementUp_Click);
			ElementDown.Click += new RoutedEventHandler(ElementDown_Click);
			ElementInput.KeyDown += new KeyEventHandler(ElementInput_KeyDown);

			ElementUp.Interval = Interval;
			ElementDown.Interval = Interval;

			UpdateVisualState();
			Value = _value;
		}

		private void ElementInput_KeyDown(object sender, KeyEventArgs e)
		{
            switch (e.Key)
            {
                case Key.Up:
                    ElementUp_Click(sender, null);
                    break;
                case Key.Down:
                    ElementDown_Click(sender, null);
                    break;
                default:
                    break;
            }
		}

		/// <summary>
		/// This event is called when the up button is clicked
		/// </summary>
		/// <param name="sender">Event source object</param>
		/// <param name="e">Event arguments</param>
		private void ElementUp_Click(object sender, RoutedEventArgs e)
		{
			Value += Step;
		}

		/// <summary>
		/// This event is called when the down button is clicked
		/// </summary>
		/// <param name="sender">Event source object</param>
		/// <param name="e">Event arguments</param>
		private void ElementDown_Click(object sender, RoutedEventArgs e)
		{
			Value -= Step;
		}

		/// <summary>
		/// This event is called when the textbox text has changed
		/// </summary>
		/// <param name="sender">Event source object</param>
		/// <param name="e">Event arguments</param>
		private void ElementInput_TextChanged(object sender, TextChangedEventArgs e)
		{
			string temp = Regex.Replace(ElementInput.Text, @"[^0-9\-]", "", RegexOptions.IgnoreCase);
            int val = 0;

			if (temp.Length == 0)
			{
				temp = "0";
			}

            int.TryParse(temp, out val);
            if (val != Value)
            {
                Value = val;
            }

			ElementInput.Text = Value.ToString();
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
