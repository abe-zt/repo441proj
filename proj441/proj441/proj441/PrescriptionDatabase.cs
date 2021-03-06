﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace proj441
{
    public class PrescriptionDatabase
    {
        readonly SQLiteAsyncConnection database;

        public PrescriptionDatabase(string path)
        {
            database = new SQLiteAsyncConnection(path);
            database.CreateTableAsync<Prescription>().Wait();
        }

        public Task<List<Prescription>> GetItemsAsync()
        {
            return database.Table<Prescription>().ToListAsync();
        }

        public Task<Prescription> GetItemAsync(int pid)
        {
            return database.Table<Prescription>().Where(i => i.PID == pid).FirstOrDefaultAsync();
        }

        public Task<int> SaveItemAsync(Prescription item)
        {
            if (item.PID != 0)
            {
                return database.UpdateAsync(item);
            }
            else
            {
                return database.InsertAsync(item);
            }
        }

        public Task<int> DeleteItemAsync(Prescription item)
        {
            return database.DeleteAsync(item);
        }
    }
}
