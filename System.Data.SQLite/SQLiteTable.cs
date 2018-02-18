namespace System.Data.SQLite
{
	public class SQLiteTable
	{
		public string TableName;

		public SQLiteColumnList Columns;

		public SQLiteTable()
		{
			//Class5.XCUF1frzK2Woy();
			this.TableName = "";
			this.Columns = new SQLiteColumnList();
			//base._002Ector();
		}

		public SQLiteTable(string name)
		{
			//Class5.XCUF1frzK2Woy();
			this.TableName = "";
			this.Columns = new SQLiteColumnList();
			//base._002Ector();
			this.TableName = name;
		}
	}
}
