/** 
 * This file is part of the OrderImportErrorWatcher project.
 * Copyright (c) 2014 Dai Nguyen
 * Author: Dai Nguyen
**/

using OrderImportErrorWatcher.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Entity;
using System;

namespace OrderImportErrorWatcher.DataAccess
{    
    public class FileImportService : DataService<FileImport>, IDisposable
    {        
        public FileImportService()
            : base()
        { }

        public FileImportService(DataContext context)
            : base(context)
        { }        

        public async Task<FileImport> GetByFileNameAsync(string filename)
        {
            return await All()
                .FirstOrDefaultAsync(t => t.FileName == filename);
        }

        public async Task<List<FileImport>> GetReadyToCheckImports(int seconds)
        {            
            string sql = @"
select  * 
from    FileImport 
where   strftime('%s', datetime('now', 'localtime')) - strftime('%s', DateDeleted) >= @second
        and DateChecked is null
";            
            var sparam = new System.Data.SQLite.SQLiteParameter("@second", seconds);
            return await Context.Database.SqlQuery<FileImport>(sql, sparam).ToListAsync();
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}
