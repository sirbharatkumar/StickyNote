using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Liquid
{
    /// <summary>
    /// A Main Menu control for building top drop-down menus
    /// </summary>
    public partial class MainMenu : BaseMenuControl
    {
        #region Visual Elements

        /// <summary> 
        /// Menu template.
        /// </summary>
        internal Menu ElementMenu { get; set; }
        internal const string ElementMenuName = "ElementMenu";

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a collection of menu items
        /// </summary>
        public ObservableCollection<MainMenuItem> Items { get; set; }

        #endregion

        #region Public Events

        public event MenuEventHandler ItemSelected;

        #endregion

        #region Constructor

        public MainMenu()
        {
            Items = new ObservableCollection<MainMenuItem>();
            Items.CollectionChanged += new NotifyCollectionChangedEventHandler(Children_CollectionChanged);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the menu item with the provided ID
        /// </summary>
        /// <param name="ID">Menu ID to look for</param>
        /// <returns>BaseMenuControl with the matching ID or null if not found</returns>
        public BaseMenuControl Get(string ID)
        {
            BaseMenuControl result = null;
            MenuItem item;

            foreach (BaseMenuControl e in Items)
            {
                if (e.ID == ID)
                {
                    result = e;
                    break;
                }

                if (e is MenuItem)
                {
                    item = (MenuItem)e;
                    if (item.Content is Menu)
                    {
                        result = ((Menu)item.Content).Get(ID);
                        if (result != null)
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Sets the enabled state of a menu item with the provided ID
        /// </summary>
        /// <param name="ID">Menu item ID</param>
        /// <param name="enabledState">Enabled state</param>
        public void SetEnabledStatus(string ID, bool enabledState)
        {
            BaseMenuControl item = Get(ID);

            if (item != null)
            {
                item.IsEnabled = enabledState;
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// This method sycronizes the Child Items collection with the rendered Canvas
        /// </summary>
        internal void SyncChildren()
        {
            int i;

            if (ElementRoot != null)
            {
                ElementMenu.Items.Clear();
                for (i = 0; i < Items.Count; i++)
                {
                    ElementMenu.Items.Add(Items[i]);
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

            ElementMenu = (Menu)GetTemplateChild(ElementMenuName);
            ElementMenu.ItemSelected += new MenuEventHandler(RaiseItemSelected);
            SyncChildren();
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SyncChildren();
        }

        #endregion

        #region Event Raising

        /// <summary>
        /// Generates a ItemSelected event to indicate an item has been clicked
        /// </summary>
        /// <param name="sender">Event source object</param>
        /// <param name="args">Event arguments</param>
        public void RaiseItemSelected(object sender, MenuEventArgs args)
        {
            if (ItemSelected != null)
            {
                ItemSelected(this, args);
            }
        }

        #endregion
    }
}
