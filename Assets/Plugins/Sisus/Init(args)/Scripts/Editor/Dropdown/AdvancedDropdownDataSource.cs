﻿using System.Collections.Generic;

namespace Sisus.Init.EditorOnly
{
    internal abstract class AdvancedDropdownDataSource
    {
        private const string kSearchHeader = "Search";

        private AdvancedDropdownItem m_MainTree;
        private AdvancedDropdownItem m_SearchTree;

        public AdvancedDropdownItem mainTree { get { return m_MainTree; } }
        public AdvancedDropdownItem searchTree { get { return m_SearchTree; } }

        public void ReloadData()
        {
            m_MainTree = GetData();
        }

        protected abstract AdvancedDropdownItem GetData();

        public void RebuildSearch(string search)
        {
            m_SearchTree = Search(search);
        }

        virtual protected AdvancedDropdownItem Search(string searchString)
        {
            if(string.IsNullOrEmpty(searchString))
                return null;

            // Support multiple search words separated by spaces.
            var searchWords = searchString.ToLower().Split(' ');

            // We keep two lists. Matches that matches the start of an item always get first priority.
            var matchesStart = new List<AdvancedDropdownItem>();
            var matchesWithin = new List<AdvancedDropdownItem>();

            foreach(var e in m_MainTree.GetSearchableElements())
            {
                var name = e.Name.ToLower().Replace(" ", "");

                var didMatchAll = true;
                var didMatchStart = false;

                // See if we match ALL the seaarch words.
                for(var w = 0; w < searchWords.Length; w++)
                {
                    var search = searchWords[w];
                    if(name.Contains(search))
                    {
                        // If the start of the item matches the first search word, make a note of that.
                        if(w == 0 && name.StartsWith(search))
                            didMatchStart = true;
                    }
                    else
                    {
                        // As soon as any word is not matched, we disregard this item.
                        didMatchAll = false;
                        break;
                    }
                }
                // We always need to match all search words.
                // If we ALSO matched the start, this item gets priority.
                if(didMatchAll)
                {
                    if(didMatchStart)
                        matchesStart.Add(e);
                    else
                        matchesWithin.Add(e);
                }
            }

            matchesStart.Sort();
            matchesWithin.Sort();

            var searchTree = new AdvancedDropdownItem(kSearchHeader, -1);
            foreach(var element in matchesStart)
            {
                searchTree.AddChild(element);
            }
            foreach(var element in matchesWithin)
            {
                searchTree.AddChild(element);
            }
            return searchTree;
        }
    }
}
