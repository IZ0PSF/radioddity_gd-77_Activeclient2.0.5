namespace System.Data.SQLite
{
	public class SQLiteColumn
	{
		public string ColumnName;

		public bool PrimaryKey;

		public ColType ColDataType;

		public bool AutoIncrement;

		public bool NotNull;

		public string DefaultValue;

		public SQLiteColumn()
		{
			//Class5.XCUF1frzK2Woy();
			this.ColumnName = "";
			this.DefaultValue = "";
			//base._002Ector();
		}

		public SQLiteColumn(string colName)
		{
			//Class5.XCUF1frzK2Woy();
			this.ColumnName = "";
			this.DefaultValue = "";
			//base._002Ector();
			this.ColumnName = colName;
			this.PrimaryKey = false;
			this.ColDataType = ColType.Text;
			this.AutoIncrement = false;
		}

		public SQLiteColumn(string colName, ColType colDataType)
		{
			//Class5.XCUF1frzK2Woy();
			this.ColumnName = "";
			this.DefaultValue = "";
			//base._002Ector();
			this.ColumnName = colName;
			this.PrimaryKey = false;
			this.ColDataType = colDataType;
			this.AutoIncrement = false;
		}

		public SQLiteColumn(string colName, bool autoIncrement)
		{
			//Class5.XCUF1frzK2Woy();
			this.ColumnName = "";
			this.DefaultValue = "";
			//base._002Ector();
			this.ColumnName = colName;
			if (autoIncrement)
			{
				this.PrimaryKey = true;
				this.ColDataType = ColType.Integer;
				this.AutoIncrement = true;
			}
			else
			{
				this.PrimaryKey = false;
				this.ColDataType = ColType.Text;
				this.AutoIncrement = false;
			}
		}

		public SQLiteColumn(string colName, ColType colDataType, bool primaryKey, bool autoIncrement, bool notNull, string defaultValue)
		{
			//Class5.XCUF1frzK2Woy();
			this.ColumnName = "";
			this.DefaultValue = "";
			//base._002Ector();
			this.ColumnName = colName;
			if (autoIncrement)
			{
				this.PrimaryKey = true;
				this.ColDataType = ColType.Integer;
				this.AutoIncrement = true;
			}
			else
			{
				this.PrimaryKey = primaryKey;
				this.ColDataType = colDataType;
				this.AutoIncrement = false;
				this.NotNull = notNull;
				this.DefaultValue = defaultValue;
			}
		}
	}
}
