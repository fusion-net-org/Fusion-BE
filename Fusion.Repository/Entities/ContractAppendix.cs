using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Entities
{
    public class ContractAppendix
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("contract_id")]
        public Guid ContractId { get; set; }

        [Column("appendix_code")]
        [StringLength(50)]
        public string? AppendixCode { get; set; }   // mã phụ lục (PL-01, PL-02)

        [Column("title")]
        [StringLength(200)]
        public string? Title { get; set; }          // tiêu đề phụ lục

        [Column("file_path")]
        public string? FilePath { get; set; }       // link file Firebase / local

        [Column("description")]
        public string? Description { get; set; }    // mô tả thêm (tuỳ chọn)

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation
        [ForeignKey("ContractId")]
        [InverseProperty("ContractAppendices")]
        public virtual Contract? Contract { get; set; }

    }
}
