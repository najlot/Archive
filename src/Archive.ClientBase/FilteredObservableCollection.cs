using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Archive.ClientBase
{
	public class FilteredObservableCollection<T> : IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
	{
		public List<T> Items { get; private set; }
		private List<T> _filteredItems;

		public FilteredObservableCollection()
		{
			Items = new List<T>();
			_filteredItems = Items;
		}

		public FilteredObservableCollection(IEnumerable<T> items)
		{
			Items = new List<T>(items);
			_filteredItems = Items;
		}

		public Func<T, bool> Filter { get; set; }

		public void Refresh()
		{
			if (Filter == null)
			{
				_filteredItems = Items;
			}
			else
			{
				_filteredItems = Items.Where(Filter).ToList();
			}

			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
		}

		public T this[int index]
		{
			get => _filteredItems[index];
			set
			{
				_filteredItems[index] = value;
				Refresh();
			}
		}

		public int Count => _filteredItems.Count;

		public bool IsReadOnly => false;

		public event NotifyCollectionChangedEventHandler CollectionChanged;
		public event PropertyChangedEventHandler PropertyChanged;

		public void Add(T item)
		{
			Items.Add(item);
			Refresh();
		}

		public void Clear()
		{
			Items.Clear();
			Refresh();
		}

		public bool Contains(T item) => _filteredItems.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => _filteredItems.CopyTo(array, arrayIndex);
		public IEnumerator<T> GetEnumerator() => _filteredItems.GetEnumerator();
		public int IndexOf(T item) => _filteredItems.IndexOf(item);

		public void Insert(int index, T item)
		{
			Items.Insert(index, item);
			Refresh();
		}

		public bool Remove(T item)
		{
			var rc = Items.Remove(item);
			Refresh();
			return rc;
		}

		public void RemoveAt(int index)
		{
			Items.RemoveAt(index);
			Refresh();
		}

		IEnumerator IEnumerable.GetEnumerator() => _filteredItems.GetEnumerator();
	}
}
