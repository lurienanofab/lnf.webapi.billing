using LNF.Models.Billing;
using LNF.Repository;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    public class ToolController : ApiController
    {
        [Route("tool/data/clean")]
        public IEnumerable<IToolDataClean> GetToolDataClean(DateTime sd, DateTime ed, int clientId = 0, int resourceId = 0)
        {
            using (DA.StartUnitOfWork())
            {
                return ServiceProvider.Current.Billing.Tool.GetToolDataClean(sd, ed, clientId, resourceId);
            }
        }

        [Route("tool/data/clean/{reservationId}")]
        public IToolDataClean GetToolDataClean(int reservationId)
        {
            using (DA.StartUnitOfWork())
            {
                return ServiceProvider.Current.Billing.Tool.GetToolDataClean(reservationId);
            }
        }

        [HttpGet, Route("tool/data/create")]
        public IEnumerable<IToolData> CreateToolData(DateTime period, int clientId = 0, int resourceId = 0)
        {
            // Does the processing without saving anything to the database.

            using (DA.StartUnitOfWork())
            {
                return ServiceProvider.Current.Billing.Tool.CreateToolData(period, clientId, resourceId);
            }
        }


        [HttpGet, Route("tool/data/create/{reservationId}")]
        public IEnumerable<IToolData> CreateToolData(int reservationId)
        {
            using (DA.StartUnitOfWork())
            {
                return ServiceProvider.Current.Billing.Tool.CreateToolData(reservationId);
            }
        }

        [Route("tool/data")]
        public IEnumerable<IToolData> GetToolData(DateTime period, int clientId = 0, int resourceId = 0)
        {
            using (DA.StartUnitOfWork())
            {
                return ServiceProvider.Current.Billing.Tool.GetToolData(period, clientId, resourceId);
            }
        }

        [Route("tool/data/{reservationId}")]
        public IEnumerable<IToolData> GetToolData(int reservationId)
        {
            using (DA.StartUnitOfWork())
            {
                return ServiceProvider.Current.Billing.Tool.GetToolData(reservationId);
            }
        }

        [HttpGet, Route("tool/create")]
        public IEnumerable<IToolBilling> CreateToolBilling(DateTime period, int clientId = 0)
        {
            // Does the same processing as BillingDataProcessStep1.PopulateToolBilling (transforms
            // a ToolData record into a ToolBilling record) without saving anything to the database.

            using (DA.StartUnitOfWork())
            {
                return ServiceProvider.Current.Billing.Tool.CreateToolBilling(period, clientId);
            }
        }

        [HttpGet, Route("tool/create/{reservationId}")]
        public IEnumerable<IToolBilling> CreateToolBilling(int reservationId)
        {
            // Does the same processing as BillingDataProcessStep1.PopulateToolBilling (transforms
            // a ToolData record into a ToolBilling record) without saving anything to the database.

            using (DA.StartUnitOfWork())
            {
                return ServiceProvider.Current.Billing.Tool.CreateToolBilling(reservationId);
            }
        }

        [Route("tool")]
        public IEnumerable<IToolBilling> GetToolBilling(DateTime period, int clientId = 0, int resourceId = 0)
        {
            using (DA.StartUnitOfWork())
            {
                return ServiceProvider.Current.Billing.Tool.GetToolBilling(period, clientId, resourceId);
            }
        }

        [Route("tool/{reservationId}")]
        public IEnumerable<IToolBilling> GetToolBilling(int reservationId)
        {
            using (DA.StartUnitOfWork())
            {
                return ServiceProvider.Current.Billing.Tool.GetToolBilling(reservationId);
            }
        }
    }
}
