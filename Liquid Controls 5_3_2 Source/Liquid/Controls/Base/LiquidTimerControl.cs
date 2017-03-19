using System;
using System.Windows.Media.Animation;

namespace Liquid
{
    public abstract partial class LiquidTimerControl : LiquidControl
    {
        #region Private Properties

        private Storyboard _timer = new Storyboard();

        #endregion

        #region Event Handling

        /// <summary>
        /// This is called when the template has been bound to the control
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _timer.Duration = TimeSpan.FromMilliseconds(20);
            _timer.Completed += new EventHandler(Tick);
            _timer.Begin();
        }

        /// <summary>
        /// This event is called periodically
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="e">Event arguments</param>
        protected virtual void Tick(object sender, EventArgs e)
        {
            _timer.Begin();
        }

        #endregion
    }
}
