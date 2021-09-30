﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CoreWebAPI2.Models
{
    [Table("TblProduct")]
    public class ProductModel
    {
        [Key]
        public int ID { get; set; }
        [Required]
        [Column(name: "ProductTitle")]
        public string Title { get; set; }
        [Required]
        [Column(name: "ProductDescription")]
        public string Description { get; set; }
        public byte[] DisplayImage { get; set; }
        public string UserID { get; set; }

    }
}
