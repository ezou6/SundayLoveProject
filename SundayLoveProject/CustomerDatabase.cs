using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SQLite;
using SundayLoveProject.Models;

namespace SundayLoveProject {
    /// <summary>
    /// Represents a database for storing and retrieving customer data.
    /// </summary>
    public class CustomerDatabase {
        SQLiteAsyncConnection Database;

        /// <summary>
        /// Initializes a new instance of the CustomerDatabase class.
        /// </summary>
        public CustomerDatabase() {
        }


        /// <summary>
        /// Initializes the database connection if it hasn't been initialized yet.
        /// </summary>
        /// <returns>A task representing the asynchronous initialization process.</returns>
        async Task Init() {
            if (Database is not null)
                return;

            //If File doesn't exist, download from the database (this will only happen on new install or if app data is cleared)
            if (!File.Exists(Constants.DatabasePath))
                App.Firebase.DownloadFileAsync(Constants.DatabaseFilename, Constants.DatabasePath);
            


            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<Customer>();

        }

        /// <summary>
        /// Retrieves all customers from the database.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of customers.</returns>
        public async Task<List<Customer>> GetCustomersAsync() {
            await Init();
            return await Database.Table<Customer>().ToListAsync();
        }

        /// <summary>
        /// Retrieves a customer from the database by their ID.
        /// </summary>
        /// <param name="id">The ID of the customer to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the retrieved customer.</returns>
        public async Task<Customer> GetCustomerAsync(int id) {
            await Init();
            return await Database.Table<Customer>().Where(i => i.ID == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Saves a customer to the database.
        /// </summary>
        /// <param name="customer">The customer object to save.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the saved customer.</returns>
        public async Task<int> SaveCustomerAsync(Customer customer) {
            await Init();
            //serialize the customers date time collection
            customer.DateTimesBlob = JsonConvert.SerializeObject(customer.DatesShopped);
            if (customer.ID != 0) {
                return await Database.UpdateAsync(customer);
            }
            else {
                return await Database.InsertAsync(customer);
            }
        }

        /// <summary>
        /// Deletes a customer from the database.
        /// </summary>
        /// <param name="customer">The customer object to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows affected.</returns>
        public async Task<int> DeleteCustomerAsync(Customer customer) {
            await Init();
            return await Database.DeleteAsync(customer);
        }

        /// <summary>
        /// Deletes all customers from the database. This method is not meant to be callled normally. REPEAT: DO NOT CALL UNLESS YOU ARE TESTING. DO NOT CALL. DO NOT CALL. REPEAT: DO NOT CALL
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of rows affected.</returns>
        public async Task<int> DeleteAllCustomersAsync() {
            await Init();
            return await Database.DeleteAllAsync<Customer>();
        }
    }
}