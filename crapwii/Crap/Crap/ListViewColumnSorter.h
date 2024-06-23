#pragma once

using namespace System;
using namespace System::Collections;	
using namespace System::Windows::Forms;

__gc class ListViewColumnSorter: public IComparer
{
	// Specifies the column that is to be sorted.
	private:
		int ColumnToSort;
		
		// Specifies the order in which to sort ('Ascending' or 'Descending').
		SortOrder OrderOfSort;
		
		// Case insensitive comparer object.
		CaseInsensitiveComparer *ObjectCompare;

	public:
		ListViewColumnSorter(void);
		~ListViewColumnSorter(void);
		int Compare(Object *x, Object *y);

		// Gets or sets the number of the column to which to apply the sorting 
  // operation (by default, this number is 0).
	public: 
		__property void set_SortColumn(int val)
		{
			ColumnToSort = (val);
		}
		__property int get_SortColumn()
		{
			return ColumnToSort;
		}

		// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
		__property void set_Order(SortOrder val)
		{
			OrderOfSort = (val);
		}
	
		__property SortOrder get_Order()
		{
			return OrderOfSort;
		}
};