using LNF.Cache;
using LNF.Data;
using LNF.Models.Billing;
using LNF.Repository.Billing;
using LNF.Repository.Scheduler;
using LNF.Scheduler;

namespace LNF.WebApi.Billing.Models
{
    public class RegularExceptionFactory
    {
        public RegularException CreateRegularException(IToolBilling tb, ReservationInvitee invitee)
        {
            var client = CacheManager.Current.GetClient(tb.ClientID);
            var res = CacheManager.Current.ResourceTree().GetResource(tb.ResourceID);
            var acct = CacheManager.Current.GetAccount(tb.AccountID);

            int inviteeClientId = 0;
            string inviteeFirst = string.Empty;
            string inviteeLast = string.Empty;

            if (invitee != null)
            {
                inviteeClientId = invitee.Invitee.ClientID;
                inviteeFirst = invitee.Invitee.FName;
                inviteeLast = invitee.Invitee.LName;
            }

            var result = new RegularException()
            {
                Period = tb.Period,
                BillingCategory = BillingCategory.Tool,
                ClientID = tb.ClientID,
                FName = client.FName,
                LName = client.LName,
                ReservationID = tb.ReservationID,
                InviteeClientID = inviteeClientId,
                InviteeFName = inviteeFirst,
                InviteeLName = inviteeLast,
                ResourceID = tb.ResourceID,
                ResourceName = res.ResourceName,
                AccountID = tb.AccountID,
                AccountName = acct.AccountName
            };

            return result;
        }

        public RegularException CreateRegularException(IRoomBilling rb)
        {
            var client = CacheManager.Current.GetClient(rb.ClientID);
            var room = CacheManager.Current.GetRoom(rb.RoomID);
            var acct = CacheManager.Current.GetAccount(rb.AccountID);

            var result = new RegularException()
            {
                Period = rb.Period,
                BillingCategory = BillingCategory.Room,
                ClientID = rb.ClientID,
                FName = client.FName,
                LName = client.LName,
                ReservationID = 0,
                InviteeClientID = 0,
                InviteeFName = string.Empty,
                InviteeLName = string.Empty,
                ResourceID = rb.RoomID,
                ResourceName = room.RoomDisplayName,
                AccountID = rb.AccountID,
                AccountName = acct.AccountName
            };

            return result;
        }
    }
}