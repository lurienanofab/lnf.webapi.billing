using System;
using System.Linq;
using LNF.CommonTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LNF.WebApi.Billing.Tests
{
    [TestClass]
    public class RoomDataImportReaderTests
    {
        [TestMethod]
        public void CanSelectRoomDataImportItems()
        {
            //var sd = DateTime.Parse("2019-03-01");
            //var ed = DateTime.Parse("2019-04-01");

            var sd = DateTime.Parse("2019-04-01");
            var ed = DateTime.Parse("2019-05-01");

            using (var reader = new RoomDataImportReader(sd, ed, 0, 0))
            {
                reader.SelectRoomDataImportItems();

                var list = reader.Items.ToList();
                var test = list.Where(x => x.EventDate >= DateTime.Parse("2019-05-01")).ToList();
                var allItems = reader.AllItems().ToList();
                var deleted = allItems.Where(x => x.Deleted).ToList();
                var deletedRooms = deleted.OrderBy(x => x.RoomName).Select(x => x.RoomName).Distinct().ToArray();
                var deletedCleanRoom = deleted.Where(x => x.RoomName == "Clean Room").ToList();
                var deletedWetChem = deleted.Where(x => x.RoomName == "Wet Chemistry").ToList();
                var test2 = list.Where(x => x.EventDate < DateTime.Parse("2019-05-01")).Max(x => x.EventDate);
                var test3 = reader.AllItems().Where(x => !x.Deleted && x.EventDate < DateTime.Parse("2019-05-01")).Max(x => x.EventDate);
                //Assert.AreEqual(7551, reader.Items.Count());

                var ds = reader.AsDataSet();
                //Assert.AreEqual(4, ds.Tables.Count);
            }
        }
    }
}
