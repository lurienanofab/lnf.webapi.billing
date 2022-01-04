using LNF.Impl.Billing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace LNF.WebApi.Billing.Tests
{
    [TestClass]
    public class WriteRoomDataProcessTests : TestBase
    {
        [TestMethod]
        public void CanGetCorrectEntries()
        {
            using (var conn = NewConnection())
            {
                conn.Open();

                var period = DateTime.Parse("2020-11-01");
                var clientId = 2063; // Matt Shea
                var roomId = 6;

                var expected = GetCountFromRoomDataClean(conn, period, clientId, roomId);

                var process = new WriteRoomDataProcess(new WriteRoomDataConfig { Connection = conn, Context = "WriteRoomDataProcessTests.CanGetCorrectEntries", Period = period, ClientID = clientId, RoomID = 0 });

                //process.DeleteExisting();

                var dtExtract = process.Extract();
                var dtTransform = process.Transform(dtExtract);

                // The process starts with rows returned by dbo.RoomData_Select @Action='PreLock' which includes the most recent rows
                // from before the given period. These rows will be ignored because RowState == DataRowState.Unchanged. They are never
                // used as far as I can tell. Maybe this is done just to get table schema. Because of this to we must ignore these rows
                // to check if entries match total entries from the RoomDataClean table and only sum those rows with where
                // RowState == DataRowState.Added. These will be the only rows added to the RoomData table.

                var actual = dtTransform.AsEnumerable().Where(x => x.RowState == DataRowState.Added && x.Field<int>("RoomID") == roomId).Sum(x => x.Field<double>("Entries"));
                Assert.AreEqual(expected, actual);

                conn.Close();
            }
        }

        [TestMethod]
        public void CanStartWriteRoomDataProcess()
        {
            using (var conn = NewConnection())
            {
                conn.Open();

                var period = DateTime.Parse("2020-12-01");
                //var clientId = 2063; // Matt Shea
                var clientId = 0; // everyone
                //var roomId = 6;

                var process = new WriteRoomDataProcess(new WriteRoomDataConfig { Connection = conn, Context = "WriteRoomDataProcessTests.CanStartWriteRoomDataProcess", Period = period, ClientID = clientId, RoomID = 0 });

                process.Start();

                conn.Close();
            }
        }

        private double GetCountFromRoomDataClean(SqlConnection conn, DateTime period, int clientId, int roomId)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM sselData.dbo.RoomDataClean WHERE ClientID = ISNULL(@clientId, ClientID) AND RoomID = ISNULL(@roomId, RoomID) AND EntryDT >= @sd AND EntryDT < @ed";

            if (clientId > 0)
                cmd.Parameters.AddWithValue("clientId", clientId);
            else
                cmd.Parameters.AddWithValue("clientId", DBNull.Value);

            if (roomId > 0)
                cmd.Parameters.AddWithValue("roomId", roomId);
            else
                cmd.Parameters.AddWithValue("roomId", DBNull.Value);

            cmd.Parameters.AddWithValue("sd", period);
            cmd.Parameters.AddWithValue("ed", period.AddMonths(1));

            var result = Convert.ToDouble(cmd.ExecuteScalar());

            return result;
        }
    }
}

