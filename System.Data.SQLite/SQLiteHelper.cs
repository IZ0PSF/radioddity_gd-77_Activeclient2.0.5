using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace System.Data.SQLite
{
	public class SQLiteHelper
	{
		private SQLiteCommand cmd;

		public SQLiteHelper(SQLiteCommand command)
		{
			//Class5.XCUF1frzK2Woy();
			//base._002Ector();
			this.cmd = command;
		}

		public DataTable GetTableStatus()
		{
			return this.Select("SELECT * FROM sqlite_master;");
		}

		public DataTable GetTableList()
		{
			DataTable tableStatus = this.GetTableStatus();
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("Tables");
			for (int i = 0; i < tableStatus.Rows.Count; i++)
			{
				string text = string.Concat(tableStatus.Rows[i]["name"]);
				if (text != "sqlite_sequence")
				{
					dataTable.Rows.Add(text);
				}
			}
			return dataTable;
		}

		public DataTable GetColumnStatus(string tableName)
		{
			return this.Select(string.Format("PRAGMA table_info(`{0}`);", tableName));
		}

		public DataTable ShowDatabase()
		{
			return this.Select("PRAGMA database_list;");
		}

		public void BeginTransaction()
		{
			this.cmd.CommandText = "begin transaction;";
			this.cmd.ExecuteNonQuery();
		}

		public void Commit()
		{
			this.cmd.CommandText = "commit;";
			this.cmd.ExecuteNonQuery();
		}

		public void Rollback()
		{
			this.cmd.CommandText = "rollback";
			this.cmd.ExecuteNonQuery();
		}

		public DataTable Select(string sql)
		{
			return this.Select(sql, new List<SQLiteParameter>());
		}

		public DataTable Select(string sql, Dictionary<string, object> dicParameters)
		{
			List<SQLiteParameter> parameters = this.method_0(dicParameters);
			return this.Select(sql, parameters);
		}

		public DataTable Select(string sql, IEnumerable<SQLiteParameter> parameters)
		{
			this.cmd.CommandText = sql;
			if (parameters != null)
			{
				foreach (SQLiteParameter parameter in parameters)
				{
					this.cmd.Parameters.Add(parameter);
				}
			}
			SQLiteDataAdapter sQLiteDataAdapter = new SQLiteDataAdapter(this.cmd);
			DataTable dataTable = new DataTable();
			sQLiteDataAdapter.Fill(dataTable);
			return dataTable;
		}

		public void Execute(string sql)
		{
			this.Execute(sql, new List<SQLiteParameter>());
		}

		public void Execute(string sql, Dictionary<string, object> dicParameters)
		{
			List<SQLiteParameter> parameters = this.method_0(dicParameters);
			this.Execute(sql, parameters);
		}

		public void Execute(string sql, IEnumerable<SQLiteParameter> parameters)
		{
			this.cmd.CommandText = sql;
			if (parameters != null)
			{
				foreach (SQLiteParameter parameter in parameters)
				{
					this.cmd.Parameters.Add(parameter);
				}
			}
			this.cmd.ExecuteNonQuery();
		}

		public object ExecuteScalar(string sql)
		{
			this.cmd.CommandText = sql;
			return this.cmd.ExecuteScalar();
		}

		public object ExecuteScalar(string sql, Dictionary<string, object> dicParameters)
		{
			List<SQLiteParameter> parameters = this.method_0(dicParameters);
			return this.ExecuteScalar(sql, parameters);
		}

		public object ExecuteScalar(string sql, IEnumerable<SQLiteParameter> parameters)
		{
			this.cmd.CommandText = sql;
			if (parameters != null)
			{
				foreach (SQLiteParameter parameter in parameters)
				{
					this.cmd.Parameters.Add(parameter);
				}
			}
			return this.cmd.ExecuteScalar();
		}

		public dataType ExecuteScalar<dataType>(string sql, Dictionary<string, object> dicParameters)
		{
			List<SQLiteParameter> list = null;
			if (dicParameters != null)
			{
				list = new List<SQLiteParameter>();
				foreach (KeyValuePair<string, object> dicParameter in dicParameters)
				{
					list.Add(new SQLiteParameter(dicParameter.Key, dicParameter.Value));
				}
			}
			return this.ExecuteScalar<dataType>(sql, (IEnumerable<SQLiteParameter>)list);
		}

		public dataType ExecuteScalar<dataType>(string sql, IEnumerable<SQLiteParameter> parameters)
		{
			this.cmd.CommandText = sql;
			if (parameters != null)
			{
				foreach (SQLiteParameter parameter in parameters)
				{
					this.cmd.Parameters.Add(parameter);
				}
			}
			return (dataType)Convert.ChangeType(this.cmd.ExecuteScalar(), typeof(dataType));
		}

		public dataType ExecuteScalar<dataType>(string sql)
		{
			this.cmd.CommandText = sql;
			return (dataType)Convert.ChangeType(this.cmd.ExecuteScalar(), typeof(dataType));
		}

		private List<SQLiteParameter> method_0(Dictionary<string, object> EDv3VCB0N75K4mxvR9)
		{
			List<SQLiteParameter> list = new List<SQLiteParameter>();
			if (EDv3VCB0N75K4mxvR9 != null)
			{
				{
					foreach (KeyValuePair<string, object> item in EDv3VCB0N75K4mxvR9)
					{
						list.Add(new SQLiteParameter(item.Key, item.Value));
					}
					return list;
				}
			}
			return list;
		}

		public string Escape(string data)
		{
			data = data.Replace("'", "''");
			data = data.Replace("\\", "\\\\");
			return data;
		}

		public void Insert(string tableName, Dictionary<string, object> dic)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (KeyValuePair<string, object> item in dic)
			{
				if (stringBuilder.Length == 0)
				{
					stringBuilder.Append("insert into ");
					stringBuilder.Append(tableName);
					stringBuilder.Append("(");
				}
				else
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append("`");
				stringBuilder.Append(item.Key);
				stringBuilder.Append("`");
				if (stringBuilder2.Length == 0)
				{
					stringBuilder2.Append(" values(");
				}
				else
				{
					stringBuilder2.Append(", ");
				}
				stringBuilder2.Append("@v");
				stringBuilder2.Append(item.Key);
			}
			stringBuilder.Append(") ");
			stringBuilder2.Append(");");
			this.cmd.CommandText = stringBuilder.ToString() + stringBuilder2.ToString();
			foreach (KeyValuePair<string, object> item2 in dic)
			{
				this.cmd.Parameters.AddWithValue("@v" + item2.Key, item2.Value);
			}
			this.cmd.ExecuteNonQuery();
		}

		public void Update(string tableName, Dictionary<string, object> dicData, string colCond, object varCond)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary[colCond] = varCond;
			this.Update(tableName, dicData, dictionary);
		}

		public void Update(string tableName, Dictionary<string, object> dicData, Dictionary<string, object> dicCond)
		{
			if (dicData.Count == 0)
			{
				throw new Exception("dicData is empty.");
			}
			StringBuilder stringBuilder = new StringBuilder();
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (KeyValuePair<string, object> dicDatum in dicData)
			{
				dictionary[dicDatum.Key] = null;
			}
			foreach (KeyValuePair<string, object> item in dicCond)
			{
				if (!dictionary.ContainsKey(item.Key))
				{
					dictionary[item.Key] = null;
				}
			}
			stringBuilder.Append("update `");
			stringBuilder.Append(tableName);
			stringBuilder.Append("` set ");
			bool flag = true;
			foreach (KeyValuePair<string, object> dicDatum2 in dicData)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append("`");
				stringBuilder.Append(dicDatum2.Key);
				stringBuilder.Append("` = ");
				stringBuilder.Append("@v");
				stringBuilder.Append(dicDatum2.Key);
			}
			stringBuilder.Append(" where ");
			flag = true;
			foreach (KeyValuePair<string, object> item2 in dicCond)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append(" and ");
				}
				stringBuilder.Append("`");
				stringBuilder.Append(item2.Key);
				stringBuilder.Append("` = ");
				stringBuilder.Append("@c");
				stringBuilder.Append(item2.Key);
			}
			stringBuilder.Append(";");
			this.cmd.CommandText = stringBuilder.ToString();
			foreach (KeyValuePair<string, object> dicDatum3 in dicData)
			{
				this.cmd.Parameters.AddWithValue("@v" + dicDatum3.Key, dicDatum3.Value);
			}
			foreach (KeyValuePair<string, object> item3 in dicCond)
			{
				this.cmd.Parameters.AddWithValue("@c" + item3.Key, item3.Value);
			}
			this.cmd.ExecuteNonQuery();
		}

		public long LastInsertRowId()
		{
			return this.ExecuteScalar<long>("select last_insert_rowid();");
		}

		public void CreateTable(SQLiteTable table)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("create table if not exists `");
			stringBuilder.Append(table.TableName);
			stringBuilder.AppendLine("`(");
			bool flag = true;
			foreach (SQLiteColumn column in table.Columns)
			{
				if (column.ColumnName.Trim().Length != 0)
				{
					if (flag)
					{
						flag = false;
					}
					else
					{
						stringBuilder.AppendLine(",");
					}
					stringBuilder.Append(column.ColumnName);
					stringBuilder.Append(" ");
					if (column.AutoIncrement)
					{
						stringBuilder.Append("integer primary key autoincrement");
					}
					else
					{
						switch (column.ColDataType)
						{
						case ColType.Text:
							stringBuilder.Append("text");
							break;
						case ColType.DateTime:
							stringBuilder.Append("datetime");
							break;
						case ColType.Integer:
							stringBuilder.Append("integer");
							break;
						case ColType.Decimal:
							stringBuilder.Append("decimal");
							break;
						case ColType.BLOB:
							stringBuilder.Append("blob");
							break;
						}
						if (column.PrimaryKey)
						{
							stringBuilder.Append(" primary key");
						}
						else if (column.NotNull)
						{
							stringBuilder.Append(" not null");
						}
						else if (column.DefaultValue.Length > 0)
						{
							stringBuilder.Append(" default ");
							if (!column.DefaultValue.Contains(" ") && column.ColDataType != 0 && column.ColDataType != ColType.DateTime)
							{
								stringBuilder.Append(column.DefaultValue);
							}
							else
							{
								stringBuilder.Append("'");
								stringBuilder.Append(column.DefaultValue);
								stringBuilder.Append("'");
							}
						}
					}
					continue;
				}
				throw new Exception("Column name cannot be blank.");
			}
			stringBuilder.AppendLine(");");
			this.cmd.CommandText = stringBuilder.ToString();
			this.cmd.ExecuteNonQuery();
		}

		public void RenameTable(string tableFrom, string tableTo)
		{
			this.cmd.CommandText = string.Format("alter table `{0}` rename to `{1}`;", tableFrom, tableTo);
			this.cmd.ExecuteNonQuery();
		}

		public void CopyAllData(string tableFrom, string tableTo)
		{
			DataTable dataTable = this.Select(string.Format("select * from `{0}` where 1 = 2;", tableFrom));
			DataTable dataTable2 = this.Select(string.Format("select * from `{0}` where 1 = 2;", tableTo));
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
			foreach (DataColumn column in dataTable.Columns)
			{
				if (dataTable2.Columns.Contains(column.ColumnName) && !dictionary.ContainsKey(column.ColumnName))
				{
					dictionary[column.ColumnName] = true;
				}
			}
			foreach (DataColumn column2 in dataTable2.Columns)
			{
				if (dataTable.Columns.Contains(column2.ColumnName) && !dictionary.ContainsKey(column2.ColumnName))
				{
					dictionary[column2.ColumnName] = true;
				}
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, bool> item in dictionary)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append("`");
				stringBuilder.Append(item.Key);
				stringBuilder.Append("`");
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append("insert into `");
			stringBuilder2.Append(tableTo);
			stringBuilder2.Append("`(");
			stringBuilder2.Append(stringBuilder.ToString());
			stringBuilder2.Append(") select ");
			stringBuilder2.Append(stringBuilder.ToString());
			stringBuilder2.Append(" from `");
			stringBuilder2.Append(tableFrom);
			stringBuilder2.Append("`;");
			this.cmd.CommandText = stringBuilder2.ToString();
			this.cmd.ExecuteNonQuery();
		}

		public void DropTable(string table)
		{
			this.cmd.CommandText = string.Format("drop table if exists `{0}`", table);
			this.cmd.ExecuteNonQuery();
		}

		public void UpdateTableStructure(string targetTable, SQLiteTable newStructure)
		{
			newStructure.TableName = targetTable + "_temp";
			this.CreateTable(newStructure);
			this.CopyAllData(targetTable, newStructure.TableName);
			this.DropTable(targetTable);
			this.RenameTable(newStructure.TableName, targetTable);
		}

		public void AttachDatabase(string database, string alias)
		{
			this.Execute(string.Format("attach '{0}' as {1};", database, alias));
		}

		public void DetachDatabase(string alias)
		{
			this.Execute(string.Format("detach {0};", alias));
		}
	}
}
