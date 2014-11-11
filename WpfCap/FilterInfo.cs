///////////////////////////////////////////////////////////////////////////////
// FilterInfo
//
// This software is released into the public domain.  You are free to use it
// in any way you like, except that you may not sell this source code.
//
// This software is provided "as is" with no expressed or implied warranty.
// I accept no liability for any damage or loss of business that this software
// may cause.
// 
// This source code is originally written by Tamir Khason (see http://blogs.microsoft.co.il/blogs/tamir
// or http://www.codeplex.com/wpfcap).
// 
// Modifications are made by Geert van Horrik (CatenaLogic, see http://blog.catenalogic.com) 
// 
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;

namespace WpfCap
{
    /// <summary>
    /// FilterInfo class
    /// </summary>
    public class FilterInfo : IComparable
    {
        #region Win32
        [DllImport("ole32.dll")]
        public static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
        public static extern int MkParseDisplayName(IBindCtx pbc, string szUserName, ref int pchEaten, out IMoniker ppmk);
        #endregion

        #region Variables
        public readonly string Name;
        public readonly string MonikerString;
        #endregion

        #region Constructor & destructor
        /// <summary>
        /// Initializes a new filter info object
        /// </summary>
        /// <param name="monikerString">Moniker string to base the filter on</param>
        public FilterInfo(string monikerString)
        {
            // Store values
            MonikerString = monikerString;
            Name = GetName(monikerString);
        }

        /// <summary>
        /// Initializes a new filter info object
        /// </summary>
        /// <param name="moniker">Moniker to base the filter on</param>
        internal FilterInfo(IMoniker moniker)
            : this(GetMonikerString(moniker))
        { }
        #endregion

        #region Methods
        /// <summary>
        /// Creates a specific filter based on the moniker
        /// </summary>
        /// <param name="filterMoniker">FilterMoniker to create the </param>
        /// <returns>Filter or null</returns>
        internal static IBaseFilter CreateFilter(string filterMoniker)
        {
            // Declare variables
            object filterObject = null;
            IBindCtx bindCtx;
            int n = 0;

            // Create binding context
            if (CreateBindCtx(0, out bindCtx) == 0)
            {
                // Parse the display name
                IMoniker moniker;
                if (MkParseDisplayName(bindCtx, filterMoniker, ref n, out moniker) == 0)
                {
                    // Bind to the object
                    Guid filterId = typeof(IBaseFilter).GUID;
                    moniker.BindToObject(null, null, ref filterId, out filterObject);

                    // Clean up
                    Marshal.ReleaseComObject(moniker);
                }

                // Clean up
                Marshal.ReleaseComObject(bindCtx);
            }

            // Return the filter
            return filterObject as IBaseFilter;
        }

        /// <summary>
        /// Gets the moniker string for a specific moniker
        /// </summary>
        /// <param name="moniker">Moniker to retrieve the moniker string of</param>
        /// <returns>Moniker string</returns>
        private static string GetMonikerString(IMoniker moniker)
        {
            string result;

            // Get the display name of the moniker
            moniker.GetDisplayName(null, null, out result);

            return result;
        }

        /// <summary>
        /// Gets the name of a specific moniker
        /// </summary>
        /// <param name="moniker">Moniker object to get the name of</param>
        /// <returns>Name of a specific moniker</returns>
        private static string GetName(IMoniker moniker)
        {
            // Declare variables
            Object bagObj = null;
            IPropertyBag bag;

            try
            {
                // Bind the moniker to storage
                Guid bagId = typeof(IPropertyBag).GUID;
                moniker.BindToStorage(null, null, ref bagId, out bagObj);
                bag = (IPropertyBag)bagObj;

                // Try to retrieve the friendly name
                object val = "";
                int hr = bag.Read("FriendlyName", ref val, IntPtr.Zero);
                if (hr != 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                // Convert to string & validate
                string result = (string)val;
                if (string.IsNullOrEmpty(result))
                {
                    throw new ApplicationException();
                }

                // Return result
                return result;
            }
            catch (Exception)
            {
                // Return empty string
                return string.Empty;
            }
            finally
            {
                // Clean up
                bag = null;
                if (bagObj != null)
                {
                    Marshal.ReleaseComObject(bagObj);
                    bagObj = null;
                }
            }
        }

        /// <summary>
        /// Gets the name of a specific moniker
        /// </summary>
        /// <param name="monikerString">Moniker string to get the name of</param>
        /// <returns>Name of a specific moniker</returns>
        private static string GetName(string monikerString)
        {
            // Declare variables
            IBindCtx bindCtx = null;
            IMoniker moniker = null;
            string name = "";
            int n = 0;

            // Create binding context
            if (CreateBindCtx(0, out bindCtx) == 0)
            {
                // Parse the display name
                if (MkParseDisplayName(bindCtx, monikerString, ref n, out moniker) == 0)
                {
                    // Get the name
                    name = GetName(moniker);

                    // Clean up
                    Marshal.ReleaseComObject(moniker);
                }

                // Clean up
                Marshal.ReleaseComObject(bindCtx);
            }

            // Return the name
            return name;
        }

        /// <summary>
        /// Compares the current object to another object
        /// </summary>
        /// <param name="value">Value to compare the current object to</param>
        /// <returns>If 0, the values are equal</returns>
        public int CompareTo(object value)
        {
            // Get the object as filter info
            FilterInfo f = (FilterInfo)value;

            // Check if we have a valid object
            if (f == null)
            {
                // No, so different
                return 1;
            }

            // Valid object, compare the names
            return (string.Compare(Name, f.Name, StringComparison.Ordinal));
        }
        #endregion
    }
}