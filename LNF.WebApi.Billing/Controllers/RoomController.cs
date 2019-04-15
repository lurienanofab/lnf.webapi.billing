using LNF.Models.Billing;
using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    // Similar to ToolController except there are no methods for single events (i.e. reservation) since there is no similar concept with room data.

    public class RoomController : ApiController
    {
        protected IProvider Provider { get; }

        public RoomController()
        {
            Provider = ServiceProvider.Current;
        }

        [Route("room/data/clean")]
        public IEnumerable<RoomDataCleanItem> GetRoomDataClean(DateTime sd, DateTime ed, int clientId = 0, int roomId = 0)
        {
            using (DA.StartUnitOfWork())
                return Provider.Billing.Room.GetRoomDataClean(sd, ed, clientId, roomId);
        }

        [HttpGet, Route("room/data/create")]
        public IEnumerable<RoomDataItem> CreateRoomData(DateTime period, int clientId = 0, int roomId = 0)
        {
            using (DA.StartUnitOfWork())
                return Provider.Billing.Room.CreateRoomData(period, clientId, roomId);
        }

        [Route("room/data")]
        public IEnumerable<RoomDataItem> GetRoomData(DateTime period, int clientId = 0, int roomId = 0)
        {
            using (DA.StartUnitOfWork())
                return Provider.Billing.Room.GetRoomData(period, clientId, roomId);
        }

        [Route("room/create")]
        public IEnumerable<RoomBillingItem> CreateRoomBilling(DateTime period, int clientId = 0)
        {
            using (DA.StartUnitOfWork())
                return Provider.Billing.Room.CreateRoomBilling(period, clientId);
        }

        [Route("room")]
        public IEnumerable<RoomBillingItem> GetRoomBilling(DateTime period, int clientId = 0, int roomId = 0)
        {
            using (DA.StartUnitOfWork())
                return Provider.Billing.Room.GetRoomBilling(period, clientId, roomId);
        }

        [Route("room/import-logs")]
        public IEnumerable<IRoomDataImportLog> GetImportLogs(DateTime sd, DateTime ed)
        {
            using (DA.StartUnitOfWork())
                return Provider.Billing.Room.GetImportLogs(sd, ed);
        }
    }
}
