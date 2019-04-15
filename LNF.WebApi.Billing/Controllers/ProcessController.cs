using LNF.CommonTools;
using LNF.Models;
using LNF.Models.Billing;
using LNF.Models.Billing.Process;
using LNF.Repository;
using System;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    /// <summary>
    /// Provides endpoints for billing processes
    /// </summary>
    public class ProcessController : ApiController
    {
        protected IProvider Provider { get; }

        public ProcessController()
        {
            // Should be replaced with constructor injection at some point.
            Provider = ServiceProvider.Current;
        }

        /// <summary>
        /// This is the entry point for billing data. Source data is retrieved from external systems (i.e. Prowatch, Scheduler, or Store) and loaded into the appropriate DataClean table. Some initial data scrubbing occurs to prepare the data for the remaining billing processes
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data/clean")]
        public BillingProcessDataCleanResult BillingProcessDataClean([FromBody] BillingProcessDataCleanCommand model)
        {
            using (DA.StartUnitOfWork())
                return Provider.Billing.Process.BillingProcessDataClean(model);
        }

        /// <summary>
        /// The second staging process where data is selected from the DataClean table and further compiled before being inserted into the Data table. After this process is complete the data is ready to be used for billing.
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data")]
        public BillingProcessDataResult BillingProcessData([FromBody] BillingProcessDataCommand model)
        {
            using (DA.StartUnitOfWork())
                return Provider.Billing.Process.BillingProcessData(model);
        }

        /// <summary>
        /// The process that loads data from the Data table into the Billing table. Final data cleanup and compilation takes place
        /// </summary>
        /// <param name="model">The process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/step1")]
        public BillingProcessStep1Result BillingProcessStep1([FromBody] BillingProcessStep1Command model)
        {
            using (DA.StartUnitOfWork())
                return Provider.Billing.Process.BillingProcessStep1(model);
        }

        [Obsolete, HttpPost, Route("process/step2")]
        public ProcessResult BillingProcessStep2([FromBody] BillingProcessStep2Command model)
        {
            if (model.Period == default(DateTime))
                throw new Exception("Missing parameter: Period");

            using (DA.StartUnitOfWork())
            {
                var result = new BillingProcessResult("BillingProcessStep2");

                if ((model.BillingCategory & BillingCategory.Tool) > 0)
                {
                    result.AddResult(new BillingProcessResult("PopulateToolBillingByAccount")
                    {
                        RowsLoaded = BillingDataProcessStep2.PopulateToolBillingByAccount(model.Period, model.ClientID)
                    });

                    result.AddResult(new BillingProcessResult("PopulateToolBillingByToolOrg")
                    {
                        RowsLoaded = BillingDataProcessStep2.PopulateToolBillingByToolOrg(model.Period, model.ClientID)
                    });
                }

                if ((model.BillingCategory & BillingCategory.Room) > 0)
                {
                    result.AddResult(new BillingProcessResult("PopulateRoomBillingByAccount")
                    {
                        RowsLoaded = BillingDataProcessStep2.PopulateRoomBillingByAccount(model.Period, model.ClientID)
                    });

                    result.AddResult(new BillingProcessResult("PopulateRoomBillingByRoomOrg")
                    {
                        RowsLoaded = BillingDataProcessStep2.PopulateRoomBillingByRoomOrg(model.Period, model.ClientID)
                    });
                }

                if ((model.BillingCategory & BillingCategory.Store) > 0)
                {
                    result.AddResult(new BillingProcessResult("PopulateStoreBillingByAccount")
                    {
                        RowsLoaded = BillingDataProcessStep2.PopulateStoreBillingByAccount(model.Period, model.ClientID)
                    });

                    result.AddResult(new BillingProcessResult("PopulateStoreBillingByItemOrg")
                    {
                        RowsLoaded = BillingDataProcessStep2.PopulateStoreBillingByItemOrg(model.Period, model.ClientID)
                    });
                }

                return result;
            }
        }

        [Obsolete, HttpPost, Route("process/step3")]
        public ProcessResult BillingProcessStep3([FromBody] BillingProcessStep3Command model)
        {
            if (model.Period == default(DateTime))
                throw new Exception("Missing parameter: Period");

            using (DA.StartUnitOfWork())
            {
                var result = new BillingProcessResult("BillingProcessStep3");

                if ((model.BillingCategory & BillingCategory.Tool) > 0)
                {
                    result.AddResult(new BillingProcessResult("PopulateToolBillingByOrg")
                    {
                        RowsLoaded = BillingDataProcessStep3.PopulateToolBillingByOrg(model.Period, model.ClientID)
                    });
                }

                if ((model.BillingCategory & BillingCategory.Room) > 0)
                {
                    result.AddResult(new BillingProcessResult("PopulateRoomBillingByOrg")
                    {
                        RowsLoaded = BillingDataProcessStep3.PopulateRoomBillingByOrg(model.Period, model.ClientID)
                    });
                }

                if ((model.BillingCategory & BillingCategory.Room) > 0)
                {
                    result.AddResult(new BillingProcessResult("PopulateStoreBillingByOrg")
                    {
                        RowsLoaded = BillingDataProcessStep3.PopulateStoreBillingByOrg(model.Period)
                    });
                }

                return result;
            }
        }

        /// <summary>
        /// Processes Billing data to determine correct subsidy amounts. [Note: Steps 2 and 3 are obsolete and should not be used.]
        /// </summary>
        /// <param name="model">The process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/step4")]
        public PopulateSubsidyBillingProcessResult BillingProcessStep4([FromBody] BillingProcessStep4Command model)
        {
            using (DA.StartUnitOfWork())
                return Provider.Billing.Process.BillingProcessStep4(model);
        }

        /// <summary>
        /// Execute finanlization steps on the Data tables
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data/finalize")]
        public DataFinalizeProcessResult BillingProcessDataFinalize([FromBody] BillingProcessDataFinalizeCommand model)
        {
            using (DA.StartUnitOfWork())
                return Provider.Billing.Process.BillingProcessDataFinalize(model);
        }

        /// <summary>
        /// Perfoms the daily load of data into the Data and DataClean tables
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data/update")]
        public DataUpdateProcessResult BillingProcessDataUpdate([FromBody] BillingProcessDataUpdateCommand model)
        {
            using (DA.StartUnitOfWork())
                return Provider.Billing.Process.BillingProcessDataUpdate(model);
        }

        /// <summary>
        /// Updates the BillingType to Remote for new remote processing entries
        /// </summary>
        /// <param name="model">An update command</param>
        /// <returns>A bool value indicating success</returns>
        [HttpPost, Route("process/remote-processing-update")]
        public bool RemoteProcessingUpdate([FromBody] RemoteProcessingUpdate model)
        {
            using (DA.StartUnitOfWork())
                return Provider.Billing.Process.RemoteProcessingUpdate(model);
        }

        [HttpDelete, Route("process/data/{billingCategory}")]
        public int DeleteData(BillingCategory billingCategory, DateTime period, int clientId = 0, int record = 0)
        {
            using (DA.StartUnitOfWork())
                return Provider.Billing.Process.DeleteData(billingCategory, period, clientId, record);
        }
    }
}
