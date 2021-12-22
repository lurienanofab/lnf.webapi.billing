using LNF.Billing;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    // Similar to ToolController except there are no methods for single events (i.e. reservation) since there is no similar concept with room data.

    public class RoomController : BillingApiController
    {
        public RoomController(IProvider provider) : base(provider) { }

        [Route("room/data/clean")]
        public IEnumerable<IRoomDataClean> GetRoomDataClean(DateTime sd, DateTime ed, int clientId = 0, int roomId = 0)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Room.GetRoomDataClean(sd, ed, clientId, roomId);
        }

        [HttpGet, Route("room/data/create")]
        public IEnumerable<IRoomData> CreateRoomData(DateTime period, int clientId = 0, int roomId = 0)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Room.CreateRoomData(period, clientId, roomId);
        }

        [Route("room/data")]
        public IEnumerable<IRoomData> GetRoomData(DateTime period, int clientId = 0, int roomId = 0)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Room.GetRoomData(period, clientId, roomId);
        }

        [Route("room/create")]
        public IEnumerable<IRoomBilling> CreateRoomBilling(DateTime period, int clientId = 0)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Room.CreateRoomBilling(period, clientId);
        }

        [Route("room")]
        public IEnumerable<IRoomBilling> GetRoomBilling(DateTime period, int clientId = 0, int roomId = 0)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Room.GetRoomBilling(period, clientId, roomId);
        }

        [Route("room/import-logs")]
        public IEnumerable<IRoomDataImportLog> GetImportLogs(DateTime sd, DateTime ed)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Room.GetImportLogs(sd, ed);
        }

        [HttpPost, Route("room/apportionment")]
        public void RoomApportionment()
        {

        }
    }
}
