// OpenDesktop - A search tool for the Windows desktop
// Copyright (C) 2005-2006, Pravin Paratey (pravinp at gmail dot com)
// http://opendesktop.berlios.de
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
using System;
using System.Text;
using System.Collections;

namespace OpenDesktop
{
    /// <summary>
    /// This class is like NameValueCollection but instead of storing string values
    /// it stores Object values. It is much more limited in functionality.
    /// </summary>
    class NameObjectCollection
    {
        private Hashtable m_htFunctions;

        public NameObjectCollection()
        {
            m_htFunctions = new Hashtable(10);
        }
        ~NameObjectCollection()
        {
            // TODO: Clear individual ArrayLists
            m_htFunctions.Clear();
        }

        /// <summary>
        /// Add a key/value pair to the NameObjectCollection
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void Add(object key, object value)
        {
            if (!m_htFunctions.ContainsKey(key))
            {
                ArrayList newlist = new ArrayList(2);
                m_htFunctions.Add(key, newlist);
            }
            ArrayList list = m_htFunctions[key] as ArrayList;
            list.Add(value);
        }

        /// <summary>
        /// Gets the value(s) associated with the key
        /// </summary>
        /// <param name="key">query key</param>
        /// <returns>ArrayList of values</returns>
        public ArrayList Get(object key)
        {
            return m_htFunctions[key] as ArrayList;
        }
    }
}
