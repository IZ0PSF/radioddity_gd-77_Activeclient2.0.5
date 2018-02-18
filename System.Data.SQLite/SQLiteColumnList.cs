using System.Collections;
using System.Collections.Generic;

namespace System.Data.SQLite
{
	public class SQLiteColumnList : IList<SQLiteColumn>, ICollection<SQLiteColumn>, IEnumerable<SQLiteColumn>, IEnumerable
	{
		private List<SQLiteColumn> _lst;

		public SQLiteColumn this[int index]
		{
			get
			{
				return this._lst[index];
			}
			set
			{
				if (this._lst[index].ColumnName != value.ColumnName)
				{
					this.method_0(value.ColumnName);
				}
				this._lst[index] = value;
			}
		}

		public int Count
		{
			get
			{
				return this._lst.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		private void method_0(string string_0)
		{
			int num = 0;
			while (true)
			{
				if (num < this._lst.Count)
				{
					if (this._lst[num].ColumnName == string_0)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			throw new Exception("Column name of \"" + string_0 + "\" is already existed.");
		}

		public int IndexOf(SQLiteColumn item)
		{
			return this._lst.IndexOf(item);
		}

		public void Insert(int index, SQLiteColumn item)
		{
			this.method_0(item.ColumnName);
			this._lst.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			this._lst.RemoveAt(index);
		}

		public void Add(SQLiteColumn item)
		{
			this.method_0(item.ColumnName);
			this._lst.Add(item);
		}

		public void Clear()
		{
			this._lst.Clear();
		}

		public bool Contains(SQLiteColumn item)
		{
			return this._lst.Contains(item);
		}

		public void CopyTo(SQLiteColumn[] array, int arrayIndex)
		{
			this._lst.CopyTo(array, arrayIndex);
		}

		public bool Remove(SQLiteColumn item)
		{
			return this._lst.Remove(item);
		}

		public IEnumerator<SQLiteColumn> GetEnumerator()
		{
			return (IEnumerator<SQLiteColumn>)(object)this._lst.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(object)this._lst.GetEnumerator();
		}

		public SQLiteColumnList()
		{
			//Class5.XCUF1frzK2Woy();
			this._lst = new List<SQLiteColumn>();
			//base._002Ector();
		}
	}
}
