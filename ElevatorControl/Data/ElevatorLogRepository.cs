using System.Data;
using ElevatorControl.Models;
using Microsoft.Data.SqlClient;

namespace ElevatorControl.Data
{
    // The ONLY class that reads or writes the log table.
    // The forms and the elevators never contain SQL, they call these two methods.
    //
    // DISCONNECTED MODEL: we fill a DataTable (a copy of the data in memory), change it,
    // and then ask the SqlDataAdapter to send the changes with Update().
    // The connection is only open for a moment, inside Fill() and Update().
    public class ElevatorLogRepository
    {
        // Only one thread may use the database code at a time
        private readonly object padlock = new object();

        // Saves one log entry (INSERT)
        public void AddLog(LogEntry entry)
        {
            lock (padlock)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(DatabaseHelper.ConnectionString))
                    using (SqlDataAdapter adapter = new SqlDataAdapter(
                        "SELECT TOP 0 LogId, ElevatorId, EventType, FloorNumber, Details, LogTime FROM dbo.ElevatorLog",
                        connection))
                    using (SqlCommandBuilder builder = new SqlCommandBuilder(adapter))   // builds the INSERT command for us
                    {
                        adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                        DataTable table = new DataTable();
                        adapter.Fill(table);                 // gets the column layout (no rows)

                        DataRow row = table.NewRow();
                        row["ElevatorId"] = entry.ElevatorId;
                        row["EventType"] = entry.EventType;
                        row["FloorNumber"] = entry.FloorNumber;
                        row["Details"] = entry.Details;
                        row["LogTime"] = entry.LogTime;
                        table.Rows.Add(row);

                        adapter.Update(table);               // sends the new row to SQL Server
                    }
                }
                catch (SqlException ex)
                {
                    throw new InvalidOperationException("Could not save the log entry: " + ex.Message, ex);
                }
            }
        }

        // Reads the latest log rows (SELECT) and returns them as a DataTable for the grid
        public DataTable GetLogs()
        {
            lock (padlock)
            {
                try
                {
                    string sql =
                        "SELECT TOP 1000 l.LogId, e.Name AS Elevator, l.LogTime, l.EventType, l.FloorNumber, l.Details " +
                        "FROM dbo.ElevatorLog AS l " +
                        "JOIN dbo.Elevators AS e ON e.ElevatorId = l.ElevatorId " +
                        "ORDER BY l.LogId DESC";

                    using (SqlConnection connection = new SqlConnection(DatabaseHelper.ConnectionString))
                    using (SqlDataAdapter adapter = new SqlDataAdapter(sql, connection))
                    {
                        DataTable table = new DataTable("ElevatorLog");
                        adapter.Fill(table);
                        return table;
                    }
                }
                catch (SqlException ex)
                {
                    throw new InvalidOperationException("Could not read the log: " + ex.Message, ex);
                }
            }
        }
    }
}
