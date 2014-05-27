/** 
 * This file is part of the OrderImportErrorWatcher project.
 * Copyright (c) 2014 Dai Nguyen
 * Author: Dai Nguyen
**/

using System;
using System.ComponentModel.DataAnnotations;

namespace OrderImportErrorWatcher.Models
{
    public class FileImport
    {
        [Key]
        public Int64 Id { get; set; }
        public string FileName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateDeleted { get; set; }
        public DateTime? DateChecked { get; set; }
        public string Result { get; set; }

        public FileImport()
        {
            DateCreated = DateTime.Now;
            Result = "";
        }
    }
}
