using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VLS_API.Model
{
    public class DataAccessLayer
    {
        SqlConnection sqlConn;
        SqlCommand SqlCmd;
        SqlDataAdapter DatAdptr;
        SqlDataReader DtRdr;
        #region Openconnection Method
        public string OpenConnection()
        {
            var configuration = GetConfiguration();
            if (sqlConn == null)
            {
                sqlConn = new SqlConnection(configuration.GetSection("ConnectionStrings").GetSection("dbConnection").Value);
            }
            if (sqlConn.State == ConnectionState.Closed)
            {
                sqlConn.Open();
            }
            SqlCmd = new SqlCommand();
            SqlCmd.CommandTimeout = 120;
            SqlCmd.Connection = sqlConn;
            return configuration.GetSection("ConnectionStrings").GetSection("dbConnection").Value;
        }
        #endregion

        IConfigurationRoot GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }

        #region CloseConnection and Dispose Connection Method
        public void CloseConnection()
        {
            if (sqlConn.State == ConnectionState.Open)
            {
                sqlConn.Close();
            }
            DisposeConnection();
        }
        #endregion
        #region DisposeConnection Method
        public void DisposeConnection()
        {
            if (sqlConn != null)
            {
                sqlConn.Dispose();
                sqlConn = null;
            }
        }
        #endregion
        #region GetDataTable Method

        public DataTable GetDataTable(string strsql)
        {
            OpenConnection();
            DatAdptr = new SqlDataAdapter(strsql, sqlConn);
            DataTable dtTble = new DataTable();
            DatAdptr.Fill(dtTble);
            DatAdptr.Dispose();
            CloseConnection();
            return dtTble;
        }

        #endregion      
        #region  Execute Stored Procedure Return DataTable
        public DataTable ExecuteStoredProcedureDataTable(string ProcName, SqlParameter[] SqlParams)
        {
            OpenConnection();
            SqlCmd.CommandType = CommandType.StoredProcedure;
            SqlCmd.CommandText = ProcName;
            SqlCmd.Parameters.Clear();
            foreach (SqlParameter thisParam in SqlParams)
            {
                SqlCmd.Parameters.Add((SqlParameter)thisParam);
            }
            SqlDataAdapter sd = new SqlDataAdapter(SqlCmd);
            DataTable dt = new DataTable();
            sd.Fill(dt);
            CloseConnection();
            return dt;
        }
        #endregion        
        #region Execute Stored Procedure Return Integer
        public int ExecuteStoredProcedureRetnInt(string ProcName, SqlParameter[] SqlParams)
        {
            OpenConnection();
            SqlCmd.CommandType = CommandType.StoredProcedure;
            SqlCmd.CommandText = ProcName;
            SqlCmd.Parameters.Clear();
            foreach (SqlParameter thisParam in SqlParams)
            {
                SqlCmd.Parameters.Add((SqlParameter)thisParam);
            }
            SqlCmd.Parameters["@returnval"].Direction = ParameterDirection.Output;
            
            SqlCmd.ExecuteNonQuery();
            string retunval = SqlCmd.Parameters["@ReturnVal"].Value.ToString();
            int returnvalue = Convert.ToInt32(SqlCmd.Parameters["@ReturnVal"].Value.ToString());
            CloseConnection();
            return returnvalue;
        }


        public int ExecuteStoredProcedureRetnScalr(string ProcName, SqlParameter[] SqlParams)
        {
            OpenConnection();
            SqlCmd.CommandType = CommandType.StoredProcedure;
            SqlCmd.CommandText = ProcName;
            SqlCmd.Parameters.Clear();
            foreach (SqlParameter thisParam in SqlParams)
            {
                SqlCmd.Parameters.Add((SqlParameter)thisParam);
            }
            SqlCmd.Parameters["@ReturnVal"].Direction = ParameterDirection.Output;
            SqlCmd.ExecuteScalar();
            int returnvalue = Convert.ToInt32(SqlCmd.Parameters["@ReturnVal"].Value.ToString());
            CloseConnection();
            return returnvalue;
        }

        #endregion
        #region Execute Stored Procedure Return void
        public void ExecuteStoredProcedure(string ProcName, SqlParameter[] SqlParams)
        {
            OpenConnection();
            SqlCmd.CommandType = CommandType.StoredProcedure;
            SqlCmd.CommandText = ProcName;
            SqlCmd.Parameters.Clear();
            foreach (SqlParameter thisParam in SqlParams)
            {
                SqlCmd.Parameters.Add((SqlParameter)thisParam);
            }
            SqlCmd.ExecuteNonQuery();
            CloseConnection();
        }

        internal DataTable ExecuteStoredProcedureDataTable(string v, SqlParameter[] param, object count)
        {
            throw new NotImplementedException();
        }
        #endregion
        public void dataInsert(string str_query)
        {
            OpenConnection();
            SqlCommand cmd = new SqlCommand(str_query,sqlConn);
            cmd.ExecuteNonQuery();
            CloseConnection();

        }

    }
}
