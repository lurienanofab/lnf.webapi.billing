using LNF.Billing;
using LNF.Billing.Process;
using LNF.Impl.Billing;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Http;

namespace LNF.WebApi.Billing.Controllers
{
    /// <summary>
    /// Provides endpoints for billing processes
    /// </summary>
    public class ProcessController : BillingApiController
    {
        public ProcessController(IProvider provider) : base(provider) { }

        /// <summary>
        /// This is the entry point for billing data. Source data is retrieved from external systems (i.e. Prowatch, Scheduler, or Store) and loaded into the appropriate DataClean table. Some initial data scrubbing occurs to prepare the data for the remaining billing processes
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data/clean")]
        public DataCleanResult BillingProcessDataClean([FromBody] DataCleanCommand model)
        {
            var result = Provider.Billing.Process.DataClean(model);
            return result;
        }

        /// <summary>
        /// The second staging process where data is selected from the DataClean table and further compiled before being inserted into the Data table. After this process is complete the data is ready to be used for billing.
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data")]
        public DataResult BillingProcessData([FromBody] DataCommand model)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Process.Data(model);
        }

        /// <summary>
        /// The process that loads data from the Data table into the Billing table. Final data cleanup and compilation takes place
        /// </summary>
        /// <param name="model">The process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/step1")]
        public Step1Result BillingProcessStep1([FromBody] Step1Command model)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Process.Step1(model);
        }

        [Obsolete, HttpPost, Route("process/step2")]
        public ProcessResult BillingProcessStep2([FromBody] Step2Command model)
        {
            if (model.Period == default)
                throw new Exception("Missing parameter: Period");

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
            {
                conn.Open();

                var result = new BillingProcessResult("BillingProcessStep2");

                var step2 = new BillingDataProcessStep2(conn);

                if ((model.BillingCategory & BillingCategory.Tool) > 0)
                {
                    result.AddResult(new BillingProcessResult("PopulateToolBillingByAccount")
                    {
                        RowsLoaded = step2.PopulateToolBillingByAccount(model.Period, model.ClientID)
                    });

                    result.AddResult(new BillingProcessResult("PopulateToolBillingByToolOrg")
                    {
                        RowsLoaded = step2.PopulateToolBillingByToolOrg(model.Period, model.ClientID)
                    });
                }

                if ((model.BillingCategory & BillingCategory.Room) > 0)
                {
                    result.AddResult(new BillingProcessResult("PopulateRoomBillingByAccount")
                    {
                        RowsLoaded = step2.PopulateRoomBillingByAccount(model.Period, model.ClientID)
                    });

                    result.AddResult(new BillingProcessResult("PopulateRoomBillingByRoomOrg")
                    {
                        RowsLoaded = step2.PopulateRoomBillingByRoomOrg(model.Period, model.ClientID)
                    });
                }

                if ((model.BillingCategory & BillingCategory.Store) > 0)
                {
                    result.AddResult(new BillingProcessResult("PopulateStoreBillingByAccount")
                    {
                        RowsLoaded = step2.PopulateStoreBillingByAccount(model.Period, model.ClientID)
                    });

                    result.AddResult(new BillingProcessResult("PopulateStoreBillingByItemOrg")
                    {
                        RowsLoaded = step2.PopulateStoreBillingByItemOrg(model.Period, model.ClientID)
                    });
                }

                result.SetEndedAt();

                conn.Close();

                return result;
            }
        }

        [Obsolete, HttpPost, Route("process/step3")]
        public ProcessResult BillingProcessStep3([FromBody] Step3Command model)
        {
            if (model.Period == default)
                throw new Exception("Missing parameter: Period");

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
            {
                conn.Open();

                var result = new BillingProcessResult("BillingProcessStep3");

                var step3 = new BillingDataProcessStep3(conn);

                if ((model.BillingCategory & BillingCategory.Tool) > 0)
                {
                    result.AddResult(new BillingProcessResult("PopulateToolBillingByOrg")
                    {
                        RowsLoaded = step3.PopulateToolBillingByOrg(model.Period, model.ClientID)
                    });
                }

                if ((model.BillingCategory & BillingCategory.Room) > 0)
                {
                    result.AddResult(new BillingProcessResult("PopulateRoomBillingByOrg")
                    {
                        RowsLoaded = step3.PopulateRoomBillingByOrg(model.Period, model.ClientID)
                    });
                }

                if ((model.BillingCategory & BillingCategory.Room) > 0)
                {
                    result.AddResult(new BillingProcessResult("PopulateStoreBillingByOrg")
                    {
                        RowsLoaded = step3.PopulateStoreBillingByOrg(model.Period)
                    });
                }

                result.SetEndedAt();

                conn.Close();

                return result;
            }
        }

        /// <summary>
        /// Processes Billing data to determine correct subsidy amounts. [Note: Steps 2 and 3 are obsolete and should not be used.]
        /// </summary>
        /// <param name="model">The process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/step4")]
        public PopulateSubsidyBillingResult BillingProcessStep4([FromBody] Step4Command model)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Process.Step4(model);
        }

        /// <summary>
        /// Execute finanlization steps on the Data tables
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data/finalize")]
        public FinalizeResult BillingProcessDataFinalize([FromBody] FinalizeCommand model)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Process.Finalize(model);
        }

        /// <summary>
        /// Perfoms the daily load of data into the Data and DataClean tables
        /// </summary>
        /// <param name="model">A process command</param>
        /// <returns>A result object</returns>
        [HttpPost, Route("process/data/update")]
        public UpdateResult BillingProcessDataUpdate([FromBody] UpdateCommand model)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Process.Update(model);
        }

        /// <summary>
        /// Updates the BillingType to Remote for new remote processing entries
        /// </summary>
        /// <param name="model">An update command</param>
        /// <returns>A bool value indicating success</returns>
        [HttpPost, Route("process/remote-processing-update")]
        public bool RemoteProcessingUpdate([FromBody] RemoteProcessingUpdate model)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Process.RemoteProcessingUpdate(model);
        }

        [HttpDelete, Route("process/data/{billingCategory}")]
        public int DeleteData(BillingCategory billingCategory, DateTime period, int clientId = 0, int record = 0)
        {
            using (StartUnitOfWork())
                return Provider.Billing.Process.DeleteData(billingCategory, period, clientId, record);
        }
    }
}
