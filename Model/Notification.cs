using System;
using System.Data.SqlClient;
using VLS_API.Model;
using System.Data;

namespace Howzu_API.Model
{
    public static class Notification
    {
        public static void AppNotification(string UserId, string LabID, string Title, string Message, string Type, string Payload, string CreatedBy)
        {
            DataAccessLayer DAL = new DataAccessLayer();
            try
            {
                SqlParameter[] param = new SqlParameter[]
                {
                new SqlParameter("@sUserAppid", UserId),
                new SqlParameter("@sTitle", Title),
                new SqlParameter("@sMessage", Message),
                new SqlParameter("@Type", Type),
                new SqlParameter("@Payload", Payload),
                new SqlParameter("@CreatedBy", CreatedBy),
                new SqlParameter("@returnval", SqlDbType.Int)
                };
                int result = DAL.ExecuteStoredProcedureRetnInt("Sp_AddAppNotificationUpdated", param);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
