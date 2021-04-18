// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2021 Michiel Hazelhof (michiel@hazelhof.nl)
// 
// MSM is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MSM is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using MSM.Data;
using MSM.Functions;

namespace MSM.Extends
{
    [ToolboxBitmap(typeof(DataGridView))]
    public class DataGridViewOptimized : DataGridView
    {
        public DataGridViewOptimized()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);

            CellBorderStyle = DataGridViewCellBorderStyle.RaisedVertical;
            RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            BorderStyle = BorderStyle.None;
            ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            RowHeadersVisible = false;

            // ReSharper disable once RedundantBaseQualifier
            base.DoubleBuffered = true;
            // ReSharper disable once RedundantBaseQualifier
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            if (!Variables.DesignMode)
            {
                EnableHeadersVisualStyles = false;

                DefaultCellStyle.BackColor = Variables.ColorPalette.ToolWindowCaptionActive.Background;
                AlternatingRowsDefaultCellStyle.BackColor = Variables.ColorPalette.ToolWindowCaptionActive.Grip;
                BackgroundColor = Variables.ColorPalette.ToolWindowCaptionActive.Background;
            }
        }

        private BindingSource _bindingSource;
        public DataTable DataTable = new();
        public void Bind()
        {
            _bindingSource = new BindingSource { DataSource = DataTable };
            DataSource = _bindingSource;
        }

        [DefaultValue(true)]
        public Boolean ComplexSearch { get; set; } = true;

        internal SemaphoreSlim DataGridViewOptimizedSemaphoreSlim = new(1, 1);
        private String _searchText = "";
        public void Search(String search)
        {
            if (DataTable == null || DataTable.Columns.Count == 0) return;

            if (InvokeRequired)
            {
                Invoke(new Action<String>(Search), search);
                return;
            }

            // WARNING since the lock gets appointed to a thread (possibly the STA) the conclusion is that a user bashing the enter key will crash because the lock doesn't work!
            DataGridViewOptimizedSemaphoreSlim.WaitUIFriendly();

            CurrentCell = null;

            Enabled = false;

            try
            {
                _searchText = search ?? "";

                DataTable.BeginLoadData();

                ThreadHelpers threading = new();
                threading.ExecuteThread(SearchThread, priority: ThreadPriority.AboveNormal);

                DataTable.EndLoadData();

                _bindingSource.ResetBindings(true);
            }
            catch (Exception exception)
            {
                Service.Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Fatal, "Could not search DataTable", exception);
            }

            Enabled = true;

            DataGridViewOptimizedSemaphoreSlim.Release();
        }
        private void SearchThread()
        {
#if !MINIMAL
            if (DataTable == null || DataTable.Columns.Count == 0) return;

            try
            {
                List<SearchItem> searchItemsFinal = new();
                Boolean isDelimited = false, negativeItem = false, inItem = false;
                Int32 startOfCurrentItem = -1;
                String currentItem = "";
                for (Int32 i = 0; i < _searchText.Length; i++)
                {
                    if (inItem)
                    {
                        if (i - 1 == startOfCurrentItem && _searchText[i] == '"' && negativeItem)
                        {
                            isDelimited = true;
                            continue;
                        }

                        if (isDelimited)
                        {
                            if (_searchText[i] != '"')
                            {
                                currentItem += _searchText[i];
                                continue;
                            }

                            isDelimited = false;
                            inItem = false;
                        }

                        if (String.IsNullOrWhiteSpace(_searchText[i].ToString()) || !inItem)
                        {
                            inItem = false;
                            searchItemsFinal.Add(new SearchItem { Negative = negativeItem, SearchText = currentItem });
                            currentItem = "";
                            continue;
                        }

                        currentItem += _searchText[i];
                    }

                    if (inItem || String.IsNullOrWhiteSpace(_searchText[i].ToString())) continue;

                    inItem = true;
                    currentItem = "";
                    negativeItem = _searchText[i] == '-';
                    isDelimited = _searchText[i] == '"';
                    startOfCurrentItem = i;
                    if (!negativeItem && !isDelimited)
                    {
                        currentItem += _searchText[i];
                    }
                }
                if (!String.IsNullOrWhiteSpace(currentItem))
                {
                    searchItemsFinal.Add(new SearchItem { Negative = negativeItem, SearchText = currentItem });
                }

                // Attempt to optimize the search
                List<SearchItem> searchItemsFinalCloned = new(searchItemsFinal.Count);
                searchItemsFinalCloned.AddRange(searchItemsFinal);
                searchItemsFinal.Clear();
                searchItemsFinal.AddRange(searchItemsFinalCloned.Where(searchItem => !searchItem.SearchText.IsNumeric(true)).OrderByDescending(searchItem => searchItem.SearchText.Length));
                searchItemsFinal.AddRange(searchItemsFinalCloned.Where(searchItem => searchItem.SearchText.IsNumeric(true)).OrderByDescending(x => x.SearchText.Length));
                
                if (String.IsNullOrWhiteSpace(_searchText))
                {
                    DataTable.DefaultView.RowFilter = "";
                    return;
                }

                List<(Boolean negative, List<String> search)> searchItems = new();
                foreach (SearchItem item in searchItemsFinal)
                {
                    List<String> searchItemsNew = new();

                    for (Int32 j = 0; j < DataTable.Columns.Count; j++)
                    {
                        String columnName = "[" + DataTable.Columns[j].ColumnName.Replace(@"\", @"\\").Replace("]", @"\]") + "]";
                        String invert = "", invert2 = "=", operator1 = "OR";
                        if (item.Negative)
                        {
                            invert = "NOT ";
                            invert2 = "<>";
                            operator1 = "AND";
                        }

                        if (DataTable.Columns[j].DataType == typeof(String))
                        {
                            searchItemsNew.Add("(ISNULL(" + columnName + ", '') " + invert + "LIKE '%" + item.SearchText.EscapeLikeValue() + "%')");
                        }
                        else if (DataTable.Columns[j].DataType == typeof(DateTime) && item.SearchText.Replace(":", "").Replace("-", "").Replace(" ", "").IsNumeric(false))
                        {
                            if (ComplexSearch)
                            {
                                searchItemsNew.Add("(CONVERT(" + columnName + ", 'System.String') " + invert + " LIKE '%" + item.SearchText.EscapeLikeValue() + "%')");
                            }
                        }
                        else if (DataTable.Columns[j].DataType == typeof(Decimal) && item.SearchText.IsDecimal(UInt32.MaxValue, Byte.MaxValue, true))
                        {
                            String extra = "";
                            if (ComplexSearch)
                            {
                                extra = " " + operator1 + " CONVERT(" + columnName + ", 'System.String') " + invert + "LIKE '%" + $"{item.SearchText.ToDecimal():0.#######}".ToString(CultureInfo.InvariantCulture) + "%'";
                            }

                            searchItemsNew.Add("(ISNULL(" + columnName + ", '') " + invert2 + " '" + $"{item.SearchText.ToDecimal():0.#######}'" + extra + ")");
                        }
                        else if (item.SearchText.IsNumeric(false) && ((DataTable.Columns[j].DataType == typeof(UInt64) && item.SearchText.ToUInt64() != 0) || (DataTable.Columns[j].DataType == typeof(UInt32) && item.SearchText.ToUInt32() != 0) || (DataTable.Columns[j].DataType == typeof(UInt16) && item.SearchText.ToUInt16() != 0)))
                        {
                            String extra = "";
                            if (ComplexSearch)
                            {
                                extra = " " + operator1 + " CONVERT(" + columnName + ", 'System.String') " + invert + "LIKE '%" + item.SearchText + "%'";
                            }

                            searchItemsNew.Add("(ISNULL(" + columnName + ", '') " + invert2 + " '" + item.SearchText + "'" + extra + ")");
                        }
                        else if (item.SearchText.IsNumeric(true) && ((DataTable.Columns[j].DataType == typeof(Int64) && item.SearchText.ToInt64() != 0) || (DataTable.Columns[j].DataType == typeof(Int32) && item.SearchText.ToInt32() != 0) || (DataTable.Columns[j].DataType == typeof(Int16) && item.SearchText.ToInt16() != 0)))
                        {
                            String extra = "";
                            if (ComplexSearch)
                            {
                                extra = " " + operator1 + " CONVERT(" + columnName + ", 'System.String') " + invert + "LIKE '%" + item.SearchText + "%'";
                            }

                            searchItemsNew.Add("(ISNULL(" + columnName + ", '') " + invert2 + " '" + item.SearchText + "'" + extra + ")");
                        }
                    }

                    searchItems.Add((item.Negative, searchItemsNew));
                }

                String search = "";
                foreach ((Boolean negative, List<String> searchList) in searchItems)
                {
                    if (!searchList.Any() || negative) continue;

                    if (!String.IsNullOrWhiteSpace(search))
                    {
                        search += " AND ";
                    }

                    search += "(" + String.Join(" OR ", searchList) + ")";
                }

                foreach ((Boolean negative, List<String> searchList) in searchItems)
                {
                    if (!searchList.Any() || !negative) continue;

                    if (!String.IsNullOrWhiteSpace(search))
                    {
                        search += " AND ";
                    }

                    search += String.Join(" AND ", searchList);
                }
                
                DataTable.DefaultView.RowFilter = search;
            }
            catch (Exception exception)
            {
                Service.Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Fatal, "Could not search DataTable", exception);
            }
#endif
        }
        
        [StructLayout(LayoutKind.Auto)]
        internal struct SearchItem
        {
            public String SearchText;
            public Boolean Negative;
        }
    }
}
