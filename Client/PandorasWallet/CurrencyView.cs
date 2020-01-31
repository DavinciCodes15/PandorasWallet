//   Copyright 2017-2019 Davinci Codes
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Also use the software for non-commercial purposes.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE

using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Pandora.Client.PandorasWallet
{
    public partial class CurrencyView : UserControl
    {
        private ColumnHeader FCurrencyHeader;
        private ColumnHeader FTickerHeader;
        private ColumnHeader FIdHeader;
        private Dictionary<long, CurrencyItem> FCurrencyItems = new Dictionary<long, CurrencyItem>();
        private Dictionary<long, ListViewItem> FListViewCache = new Dictionary<long, ListViewItem>();
        private bool? FSearching;
        private object FListLock = new object();

        public event ItemCheckedEventHandler OnItemChecked;

        public event EventHandler OnSelectedIndexChanged;

        public ImageList ImageList => FImageList;

        public string CurrentFilterApplied { get; private set; }

        public CurrencyView()
        {
            InitializeComponent();
            CurrencyIds = new long[0];
            FListView.Columns.Clear();
            SetVisualOptions((VisualOptionFlags.CurrencyNameVisible | VisualOptionFlags.TickerColunmVisible | VisualOptionFlags.IconVisible));
        }

        [Flags]
        public enum VisualOptionFlags
        {
            CurrencyNameVisible = 1, TickerColunmVisible = 2, IdColumnVisable = 4, IconVisible = 8, IncludeTickerWithCurrencyName = 16, UseCheckBoxes = 32
        }

        public void SetVisualOptions(VisualOptionFlags aVisualOptionFlags, string[] aCustomColumnNameList = null)
        {
            if (CurrencyIds.Length != 0)
            {
                throw new InvalidOperationException("CurrencyView control can not change unless no items exist.");
            }

            CurrencyNameVisible = aVisualOptionFlags.HasFlag(VisualOptionFlags.CurrencyNameVisible);
            IdColumnVisable = aVisualOptionFlags.HasFlag(VisualOptionFlags.IdColumnVisable);
            TickerColunmVisible = aVisualOptionFlags.HasFlag(VisualOptionFlags.TickerColunmVisible);
            IconVisible = aVisualOptionFlags.HasFlag(VisualOptionFlags.IconVisible);
            IncludeTickerWithCurrencyName = aVisualOptionFlags.HasFlag(VisualOptionFlags.IncludeTickerWithCurrencyName);
            UseCheckBoxes = aVisualOptionFlags.HasFlag(VisualOptionFlags.UseCheckBoxes);
            FListView.SmallImageList = null;
            FListView.LargeImageList = null;
            FListView.Columns.Clear();
            FImageList.Images.Clear();
            if (CurrencyNameVisible)
            {
                FCurrencyHeader = FListView.Columns.Add("Currency Name", 300, HorizontalAlignment.Left);
            }

            if (TickerColunmVisible)
            {
                FTickerHeader = FListView.Columns.Add("Ticker", 60, HorizontalAlignment.Left);
            }

            if (IdColumnVisable)
            {
                FIdHeader = FListView.Columns.Add("ID", 50, HorizontalAlignment.Left);
            }

            if (IconVisible)
            {
                FListView.SmallImageList = FImageList;
                FListView.LargeImageList = FImageList;
            }
            if (UseCheckBoxes)
            {
                FListView.CheckBoxes = true;
                FListView.MultiSelect = true;
            }
            if (aCustomColumnNameList != null)
            {
                foreach (string lName in aCustomColumnNameList)
                {
                    FListView.Columns.Add(lName, 90, HorizontalAlignment.Right);
                }
            }
        }

        public void ApplyFilter(string aFilter)
        {
            try
            {
                if (string.IsNullOrEmpty(aFilter)) throw new ArgumentException(nameof(aFilter), "Filter must be a value");
                List<long> lNotFoundKeys = new List<long>();
                List<long> lFoundKeys = new List<long>();
                foreach (var lCurrencyItem in FCurrencyItems.Values)
                {
                    var lNameMatch = System.Text.RegularExpressions.Regex.IsMatch(lCurrencyItem.Name, aFilter, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    var lTickerMatch = System.Text.RegularExpressions.Regex.IsMatch(lCurrencyItem.Ticker, aFilter, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (lNameMatch || lTickerMatch) lFoundKeys.Add(lCurrencyItem.Id);
                    else lNotFoundKeys.Add(lCurrencyItem.Id);
                }
                var lListViewItems = FListView.Items.Cast<ListViewItem>();
                foreach (long ID in lFoundKeys)
                {
                    bool lHaveItem = lListViewItems.Any(lstItem => lstItem.ImageKey == ID.ToString());
                    if (!lHaveItem)
                    {
                        var lCurrencyItem = FCurrencyItems[ID];
                        FListView.Items.Add(FListViewCache[ID]);
                    }
                }
                foreach (long ID in lNotFoundKeys)
                {
                    bool lHaveItem = lListViewItems.Any(lstItem => lstItem.ImageKey == ID.ToString());
                    if (lHaveItem)
                        FListView.Items.Remove(FListViewCache[ID]);
                }
                CurrentFilterApplied = aFilter;
            }
            catch
            {
                ClearFilter();
                throw;
            }
        }

        public void ClearFilter()
        {
            if (string.IsNullOrEmpty(CurrentFilterApplied)) return;
            FListView.Items.Clear();
            FListView.Items.AddRange(FListViewCache.Values.ToArray());
            CurrentFilterApplied = string.Empty;
        }

        public bool UpdateCurrency(long aCurrencyID, string aCurrencyName, string aTicker, string[] aCustomColumnValues = null)
        {
            ListViewItem lListViewItem = null;
            foreach (var lItem in FListView.Items)
                if (((lItem as ListViewItem).Tag as CurrencyItem).Id == aCurrencyID)
                    lListViewItem = lItem as ListViewItem;
            if (lListViewItem != null)
            {
                List<string> lColumnValues = new List<string>();
                //Updating the icon may be too disruptive
                //when the user reloads the program then we can change it.
                //if (FImageList.Images.ContainsKey(aCurrencyID.ToString()))
                //    FImageList.Images.RemoveByKey(aCurrencyID.ToString());
                //FImageList.Images.Add(aCurrencyID.ToString(), aCurrencyIcon);
                if (CurrencyNameVisible)
                    lColumnValues.Add(aCurrencyName);

                if (TickerColunmVisible)
                    lColumnValues.Add(aTicker);

                if (IdColumnVisable)
                    lColumnValues.Add(aCurrencyID.ToString());

                if (IncludeTickerWithCurrencyName)
                    lColumnValues[0] = string.Format("{0} ({1})", aCurrencyName, aTicker);

                lColumnValues.AddRange(aCustomColumnValues);
                lListViewItem.Text = lColumnValues.First();
                for (int i = 1; i < lColumnValues.Count; i++)
                    lListViewItem.SubItems[i].Text = lColumnValues[i];
            }
            return lListViewItem != null;
        }

        public void RemoveCurrency(long aCurrencyID)
        {
           lock(FListLock)
            {
                FCurrencyItems.Remove(aCurrencyID);
                FListViewCache.Remove(aCurrencyID);
                var lListViewItem = FListView.Items.Find(aCurrencyID.ToString(), false).FirstOrDefault();
                if (lListViewItem != null)
                {
                    if (SelectedCurrencyId == aCurrencyID)
                    {
                        int lLastItemIdex = FListView.Items.IndexOf(lListViewItem);
                        if (lLastItemIdex == 0)
                            FListView.Items[lLastItemIdex + 1].Selected = true;
                        else
                            FListView.Items[lLastItemIdex - 1].Selected = true;
                    }
                    FListView.Items.Remove(lListViewItem);
                }
            }
        }

        public void AddCurrency(long aCurrencyID, string aCurrencyName, string aCurrencySymbol, Icon aCurrencyIcon, string[] aCustomColumnValues = null)
        {
            CurrencyItem lCurrencyItem = new CurrencyItem(aCurrencyID, aCurrencyName, aCurrencySymbol, aCustomColumnValues);
            if (FCurrencyItems.TryGetValue(aCurrencyID, out CurrencyItem lSavedItem) && lSavedItem.AreEqual(lCurrencyItem))
                return;
            lock (FListLock)
            {
                FCurrencyItems[aCurrencyID] = lCurrencyItem;
                ListViewItem lListItem = ConstructListViewCurrencyItem(lCurrencyItem, aCurrencyIcon);
                FListViewCache[aCurrencyID] = lListItem;
                if (FSearching.HasValue && FSearching.Value)
                    AddToCurrencyList(lListItem, true);
                else
                    AddToCurrencyList(lListItem);
            }
        }

        public void ClearCurrencies()
        {
            lock (FListLock)
            {
                FImageList.Images.Clear();
                FListView.Items.Clear();
                FCurrencyItems.Clear();
                FListViewCache.Clear();
            }
        }

        public delegate bool SearchFunction(ListView aListView, Dictionary<long, ListViewItem> aCache);

        public void ExecuteSearch(SearchFunction aSearchFunction)
        {
            lock (FListLock)
            {
                FSearching = aSearchFunction?.Invoke(FListView, FListViewCache);

                if (FSearching.HasValue && !FSearching.Value)
                {
                    FListView.Items.Clear();
                    foreach (ListViewItem it in FListViewCache.Values)
                    {
                        AddToCurrencyList(it);
                    }
                }
            }
        }

        internal void UpdateStatus(List<Tuple<string, long>> aCoins, Dictionary<uint, ClientLib.CurrencyStatusItem> aCoinStatus)
        {
            lock (FListLock)
            {
                var lResult = from c in aCoins
                              join s in aCoinStatus on c.Item2 equals s.Key
                              select new { c.Item1, s.Value };

                foreach (var item in lResult)
                {
                    var lItem = FListView.Items.Cast<ListViewItem>().Where(x => x.Text == item.Item1).FirstOrDefault()?.SubItems.Cast<dynamic>().LastOrDefault();
                    if (lItem != null)
                    {
                        lItem.Text = item.Value.Status.ToString();
                    }
                }
            }
        }

        private ListViewItem ConstructListViewCurrencyItem(CurrencyItem aItem, Icon aCurrencyIcon)
        {
            if (FImageList.Images.ContainsKey(aItem.Id.ToString()))
                FImageList.Images.RemoveByKey(aItem.Id.ToString());
            FImageList.Images.Add(aItem.Id.ToString(), aCurrencyIcon);
            List<string> lColumnValues = new List<string>();
            if (CurrencyNameVisible)
                lColumnValues.Add(aItem.Name);
            if (TickerColunmVisible)
                lColumnValues.Add(aItem.Ticker);
            if (IdColumnVisable)
                lColumnValues.Add(aItem.Id.ToString());
            if (IncludeTickerWithCurrencyName)
                lColumnValues[0] = string.Format("{0} ({1})", aItem.Name, aItem.Ticker);
            lColumnValues.AddRange(aItem.CustomValues);
            string lCurrencyID = aItem.Id.ToString();
            ListViewItem lListViewItem = new ListViewItem();
            lListViewItem.Text = lColumnValues.First();
            lListViewItem.ImageKey = lCurrencyID;
            lListViewItem.Name = lCurrencyID;
            lListViewItem.Tag = aItem;
            for (int i = 1; i < lColumnValues.Count; i++)
                lListViewItem.SubItems.Add(lColumnValues[i]);
            return lListViewItem;
        }

        private void AddToCurrencyList(ListViewItem aItem, bool aOnlyReplace = false)
        {
            bool lFound = false;
            for (int it = 0; it < FListView.Items.Count; it++)
            {
                if (FListView.Items[it].ImageKey == aItem.ImageKey)
                {
                    lFound = true;
                    FListView.Items.RemoveAt(it);
                    break;
                }
            }

            if (!lFound && aOnlyReplace)
            {
                return;
            }

            FListView.Items.Add(aItem);
        }

        public void ChangeColumnWidth(int aColumnIndex, int aWidth)
        {
            FListView.Columns[aColumnIndex].Width = aWidth;
        }

        public long SelectedCurrencyId
        {
            get
            {
                if (FListView.SelectedItems.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return Convert.ToInt64(FListView.SelectedItems[0].ImageKey);
                }
            }

            set
            {
                if (value == 0)
                {
                    return;
                }

                if (!FCurrencyItems.ContainsKey(value))
                {
                    throw new ArgumentException(string.Format("CurrencyView ID {0} does not exist.", value));
                }

                ListViewItem lItem = FListView.Items.Cast<ListViewItem>().Where(x => x.ImageKey == value.ToString()).FirstOrDefault();
                if (lItem != null)
                {
                    lItem.Selected = true;
                }
            }
        }

        public long[] CheckedCurrencyIds
        {
            get
            {
                List<long> lCheckedItems = new List<long>();

                if (FListView.CheckedItems.Count > 0)
                {
                    foreach (ListViewItem it in FListView.CheckedItems)
                    {
                        lCheckedItems.Add(Convert.ToInt64(it.ImageKey));
                    }
                }
                return lCheckedItems.ToArray();
            }
        }

        public void CheckAllItems(bool select = true)
        {
            if (!UseCheckBoxes) throw new InvalidOperationException("This method only can be used when UseCheckBoxes property is set to true");
            foreach (ListViewItem lItem in FListView.Items)
                lItem.Checked = select;
        }

        public long[] CurrencyIds { get; set; }

        public bool IdColumnVisable { get; private set; }

        public bool TickerColunmVisible { get; private set; }

        public bool CurrencyNameVisible { get; private set; }

        public bool IconVisible { get; private set; }

        public bool IncludeTickerWithCurrencyName { get; private set; }

        public bool UseCheckBoxes { get; private set; }

        public class CurrencyItem
        {
            public CurrencyItem(long aCurrencyID, string aCurrencyName, string aCurrencySymbol, string[] aCustomValues)
            {
                Id = aCurrencyID;
                Name = aCurrencyName;
                Ticker = aCurrencySymbol;
                if (aCustomValues == null)
                {
                    CustomValues = new string[0];
                }
                else
                {
                    CustomValues = aCustomValues;
                }
            }

            public bool AreEqual(CurrencyItem aObj)
            {
                return (aObj.Id == Id && aObj.Name == Name && aObj.Ticker == Ticker && aObj.CustomValues.SequenceEqual(CustomValues));
            }

            public long Id { get; set; }

            public string Name { get; set; }

            public string Ticker { get; set; }

            public string[] CustomValues { get; set; }
        }

        private void FListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            OnItemChecked?.Invoke(sender, e);
        }

        private void FListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectedIndexChanged?.Invoke(sender, e);
        }
    }
}