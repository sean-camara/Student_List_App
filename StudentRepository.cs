using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using MAUIApp7.Models;

namespace MAUIApp7
{
    public class StudentRepository
    {
        private readonly SQLiteAsyncConnection _db;

        public string StatusMessage { get; private set; }

        public StudentRepository(string dbPath)
        {
            // initialize async connection and ensure table exists
            _db = new SQLiteAsyncConnection(dbPath);
            // Create the table synchronously here to ensure DB is ready for immediate calls
            _db.CreateTableAsync<Student>().Wait();
        }

        public async Task<Student> AddNewStudentAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Valid name required.");

                var student = new Student { Name = name.Trim() };
                var result = await _db.InsertAsync(student).ConfigureAwait(false);

                // SQLite-net returns rows inserted; id is set on the object
                StatusMessage = $"Record added (Name: {student.Name})";
                return student;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return null;
            }
        }

        public async Task<List<Student>> GetSectionAsync(int offset = 0, int limit = 100)
        {
            try
            {
                // Use query with Skip/Take for paging
                var query = _db.Table<Student>().OrderBy(s => s.Id).Skip(offset).Take(limit);
                var list = await query.ToListAsync().ConfigureAwait(false);

                StatusMessage = $"{list.Count} record(s) returned.";
                return list;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return new List<Student>();
            }
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            try
            {
                if (student == null) throw new ArgumentNullException(nameof(student));
                if (string.IsNullOrWhiteSpace(student.Name)) throw new ArgumentException("Valid name required.");

                var rows = await _db.UpdateAsync(student).ConfigureAwait(false);
                StatusMessage = rows == 0 ? "No record updated." : $"Record updated (Name: {student.Name})";
                return rows > 0;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            try
            {
                var rows = await _db.DeleteAsync<Student>(id).ConfigureAwait(false);
                StatusMessage = rows == 0 ? "No record deleted." : $"Record deleted (ID: {id})";
                return rows > 0;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                return false;
            }
        }
    }
}