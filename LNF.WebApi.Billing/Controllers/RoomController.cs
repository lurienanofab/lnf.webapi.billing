﻿using LNF.CommonTools;
using LNF.Models.Billing;
using LNF.Repository;
using LNF.Repository.Billing;
using LNF.Repository.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    // Similar to ToolController except there are no methods for single events (i.e. reservation) since there is no similar concept with room data.

    public class RoomController : ApiController
    {
        [Route("room/data/clean")]
        public IEnumerable<RoomDataCleanItem> GetRoomDataClean(DateTime sd, DateTime ed, int clientId = 0, int roomId = 0)
        {
            var query = DA.Current.Query<RoomDataClean>()
                .Where(x => x.EntryDT < ed && x.ExitDT > sd
                    && x.ClientID == (clientId > 0 ? clientId : x.ClientID)
                    && x.RoomID == (roomId > 0 ? roomId : x.RoomID));

            var result = query.CreateRoomDataCleanItems();

            return result;
        }

        [HttpGet, Route("room/data/create")]
        public IEnumerable<RoomDataItem> CreateRoomData(DateTime period, int clientId = 0, int roomId = 0)
        {
            // Does the processing without saving anything to the database.

            var proc = new WriteRoomDataProcess(period, clientId, roomId);
            var dtExtract = proc.Extract();
            var dtTransform = proc.Transform(dtExtract);

            var result = dtTransform.AsEnumerable().Select(x => new RoomDataItem
            {
                RoomDataID = x.Field<int>("RoomDataID"),
                Period = x.Field<DateTime>("Period"),
                ClientID = x.Field<int>("ClientID"),
                RoomID = x.Field<int>("RoomID"),
                ParentID = x.Field<int?>("ParentID"),
                PassbackRoom = x.Field<bool>("PassbackRoom"),
                EvtDate = x.Field<DateTime>("EvtDate"),
                AccountID = x.Field<int>("AccountID"),
                Entries = x.Field<double>("Entries"),
                Hours = x.Field<double>("Hours"),
                Days = x.Field<double>("Days"),
                Months = x.Field<double>("Months"),
                DataSource = x.Field<int>("DataSource"),
                HasToolUsage = x.Field<bool>("HasToolUsage"),
            }).ToList();

            return result;
        }

        [Route("room/data")]
        public IEnumerable<RoomDataItem> GetRoomData(DateTime period, int clientId = 0, int roomId = 0)
        {
            var query = DA.Current.Query<RoomData>()
                .Where(x => x.Period == period
                    && x.ClientID == (clientId > 0 ? clientId : x.ClientID)
                    && x.RoomID == (roomId > 0 ? roomId : x.RoomID));

            var result = query.CreateRoomDataItems();

            return result;
        }

        [Route("room/create")]
        public IEnumerable<RoomBillingItem> CreateRoomBilling(DateTime period, int clientId = 0)
        {
            // Does the same processing as BillingDataProcessStep1.PopulateRoomBilling (transforms
            // a RoomData record into a RoomBilling record) without saving anything to the database.

            DataSet ds;
            DataTable dt;

            var useParentRooms = bool.Parse(Utility.GetRequiredAppSetting("UseParentRooms"));
            var temp = period == DateTime.Now.FirstOfMonth();

            var result = new List<RoomBillingItem>();

            ds = BillingDataProcessStep1.GetRoomData(period);
            dt = BillingDataProcessStep1.LoadRoomBilling(ds, period, clientId, temp);
            result.AddRange(CreateRoomBillingItems(dt, temp));

            if (useParentRooms)
            {
                ds = BillingDataProcessStep1.GetRoomData(period, BillingDataProcessStep1.FOR_PARENT_ROOMS);
                dt = BillingDataProcessStep1.LoadRoomBilling(ds, period, clientId, temp);
                result.AddRange(CreateRoomBillingItems(dt, temp));
            }

            return result;
        }

        [Route("room")]
        public IEnumerable<RoomBillingItem> GetRoomBilling(DateTime period, int clientId = 0, int roomId = 0)
        {
            var temp = period == DateTime.Now.FirstOfMonth();

            var query = GetRoomBillingQuery(temp).Where(x =>
                x.Period == period
                && x.ClientID == (clientId > 0 ? clientId : x.ClientID)
                && x.RoomID == (roomId > 0 ? roomId : x.RoomID));

            var result = query.CreateRoomBillingItems();

            return result;
        }

        private IQueryable<IRoomBilling> GetRoomBillingQuery(bool temp)
        {
            if (temp)
                return DA.Current.Query<RoomBillingTemp>();
            else
                return DA.Current.Query<RoomBilling>();
        }

        private IEnumerable<RoomBillingItem> CreateRoomBillingItems(DataTable source, bool temp)
        {
            return source.AsEnumerable().Select(x => new RoomBillingItem
            {
                RoomBillingID = x.Field<int>("RoomBillingID"),
                Period = x.Field<DateTime>("Period"),
                ClientID = x.Field<int>("ClientID"),
                RoomID = x.Field<int>("RoomID"),
                AccountID = x.Field<int>("AccountID"),
                ChargeTypeID = x.Field<int>("ChargeTypeID"),
                BillingTypeID = x.Field<int>("BillingTypeID"),
                OrgID = x.Field<int>("OrgID"),
                ChargeDays = x.Field<decimal>("ChargeDays"),
                PhysicalDays = x.Field<decimal>("PhysicalDays"),
                AccountDays = x.Field<decimal>("AccountDays"),
                Entries = x.Field<decimal>("Entries"),
                Hours = x.Field<decimal>("Hours"),
                IsDefault = x.Field<bool>("IsDefault"),
                RoomRate = x.Field<decimal>("RoomRate"),
                EntryRate = x.Field<decimal>("EntryRate"),
                MonthlyRoomCharge = x.Field<decimal>("MonthlyRoomCharge"),
                RoomCharge = x.Field<decimal>("RoomCharge"),
                EntryCharge = x.Field<decimal>("EntryCharge"),
                SubsidyDiscount = x.Field<decimal>("SubsidyDiscount"),
                IsTemp = x.Field<bool>("IsTemp")
            }).ToList();
        }
    }
}
