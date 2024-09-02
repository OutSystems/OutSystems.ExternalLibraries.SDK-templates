using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace StoredProcedure {

    public class StoredProcedure : IStoredProcedure
    {
        public List<Customer> Exec(string username, string password, string catalog, string value, out string connectionString, out string exception) {
            List<Customer> customers = new List<Customer>();
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            var privateGateway = Environment.GetEnvironmentVariable("SECURE_GATEWAY")+",1433";

            builder.DataSource = privateGateway;
            builder.UserID = username;
            builder.Password = password;
            builder.InitialCatalog = catalog;
            builder.TrustServerCertificate = true;
            connectionString = builder.ConnectionString;
            exception = "";
            try{
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString)) {
                    connection.Open();
                    string sql = "EXEC GetCustomerInfo @CustomerAge = 25";
                    
                    using (SqlCommand command = new SqlCommand(sql, connection)) {
                        using (SqlDataReader reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                Customer customer = new Customer {
                                    id = reader.GetInt32(0),
                                    name = reader.GetString(1),
                                    age = reader.GetInt32(2),
                                    address = reader.GetString(3),
                                    salary = reader.GetDecimal(4)
                                };
                                customers.Add(customer);
                            }
                        }
                    } 
                }
                return customers;
            } catch (Exception e) {
                connectionString = builder.ConnectionString;
                exception = e.ToString();
                return customers;
            }
            
        }

    }
}