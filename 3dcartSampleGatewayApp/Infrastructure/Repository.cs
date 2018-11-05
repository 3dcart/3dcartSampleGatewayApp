using _3dcartSampleGatewayApp.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using _3dcartSampleGatewayApp.Helpers;
using _3dcartSampleGatewayApp.Models.Cart;

namespace _3dcartSampleGatewayApp.Infrastructure
{
    public class Repository : IRepository
    {
        private readonly Dictionary<int, int> RetryWaittingTime = new Dictionary<int, int>
        {
            { 0, 2000 },
            { 1, 3000 },
            { 2, 4000 }
        };

        private readonly int maxRetryNumber = 3;
        
        public List<CheckoutRequest> GetCheckoutRequests(int id, int retryNumber = 0)
        {

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connStr"].ConnectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand command = new SqlCommand("PASTE HERE THE STORE PROCEDURE'S NAME TO RETRIEVE THE CHECKOUT REQUEST INFORMATION", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                        DataTable dt = new DataTable();
                        SqlDataAdapter da = new SqlDataAdapter(command);

                        da.Fill(dt);

                        var requests = new List<CheckoutRequest>();

                        foreach (DataRow row in dt.Rows)
                        {
                            var request = new CheckoutRequest();
                            request.id = Convert.ToInt32(row["id"]);
                            request.orderid = Convert.ToInt32(row["orderid"]);
                            request.invoice = row["invoice"].ToString();
                            request.username = Utils.Decrypt(row["username"].ToString());
                            request.password = Utils.Decrypt(row["password"].ToString());
                            request.errorurl = row["errorurl"].ToString();
                            request.notificationurl = row["notificationurl"].ToString();
                            request.returnurl = row["returnurl"].ToString();
                            request.amounttotal = Convert.ToInt32(row["amounttotal"]);
                            request.status = (Status)(row["status"]);
                            requests.Add(request);
                        }

                        return requests;
                    }
                }
                catch (Exception ex)
                {
                    conn.Close();

                    if (retryNumber < maxRetryNumber)
                    {
                        Thread.Sleep(RetryWaittingTime[retryNumber]);
                        return GetCheckoutRequests(id, retryNumber + 1);
                    }
                    else
                    {
                        throw new Exception("ERROR EXECUTING STORED PROCEDURE: GetCheckoutRequests - " + ex.Message, ex.InnerException);
                    }

                }
            }

        }
        
        public int SaveCheckoutRequest(CheckoutRequest request, int retryNumber = 0)
        {
            int id = 0; 
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connStr"].ConnectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand command = new SqlCommand("PASTE HERE THE STORE PROCEDURE'S NAME TO PERSIST THE CURRENT CHECKOUT REQUEST INFORMATION", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@orderid", SqlDbType.Int).Value = request.orderid;
                        command.Parameters.Add("@invoice", SqlDbType.NVarChar, 50).Value = request.invoice;
                        command.Parameters.Add("@username", SqlDbType.NVarChar, 250).Value = Utils.Encrypt(request.username);
                        command.Parameters.Add("@password", SqlDbType.NVarChar, 250).Value = Utils.Encrypt(request.password);
                        command.Parameters.Add("@errorurl", SqlDbType.NVarChar, 500).Value = request.errorurl;
                        command.Parameters.Add("@notificationurl", SqlDbType.NVarChar, 500).Value = request.notificationurl;
                        command.Parameters.Add("@returnurl", SqlDbType.NVarChar, 500).Value = request.returnurl;
                        command.Parameters.Add("@amounttotal", SqlDbType.Int).Value = request.amounttotal;
                        command.Parameters.Add("@status", SqlDbType.TinyInt).Value = Status.Pending;
                        command.Parameters.Add("@id", SqlDbType.Int).Direction = ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        id = (int)command.Parameters["@id"].Value;
                    }
                }
                catch (Exception ex)
                {
                    conn.Close();

                    if (retryNumber < maxRetryNumber)
                    {
                        Thread.Sleep(RetryWaittingTime[retryNumber]);
                        SaveCheckoutRequest(request, retryNumber + 1);
                    }
                    else
                    {
                        throw new Exception("ERROR EXECUTING STORED PROCEDURE: SaveCheckoutRequest - " + ex.Message, ex.InnerException);
                    }

                }
            }

            return id;

        }



        public bool UpdateCheckoutRequestStatus(int id, Status newStatus, int retryNumber = 0)
        {
            bool result = false;

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connStr"].ConnectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand command = new SqlCommand("PASTE HERE THE STORE PROCEDURE'S NAME TO UPDATE THE CHECKOUT REQUEST STATUS", conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@id", SqlDbType.Int).Value = id;
                        command.Parameters.Add("@status", SqlDbType.TinyInt).Value = newStatus;
                        command.ExecuteNonQuery();
                    }

                    result = true;
                }
                catch (Exception ex)
                {
                    conn.Close();

                    if (retryNumber < maxRetryNumber)
                    {
                        Thread.Sleep(RetryWaittingTime[retryNumber]);
                        UpdateCheckoutRequestStatus(id, newStatus, retryNumber + 1);
                    }
                    else
                    {
                        throw new Exception("ERROR EXECUTING STORED PROCEDURE: UpdateCheckoutRequestStatus - " + ex.Message, ex.InnerException);
                    }

                }
            }

            return result;
        }



    }
       
}