using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Entities
{
    public class Contract
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("contract_code")]
        [StringLength(50)]
        public string ContractCode { get; set; } = string.Empty;

        [Column("contract_name")]
        [StringLength(50)]
        public string ContractName { get; set; } = string.Empty;

        [Column("project_request_id")]
        public Guid ProjectRequestId { get; set; }

        [Column("attachment")]
        public string? Attachment { get; set; }

        [Column("effective_date")]
        public DateOnly EffectiveDate { get; set; }

        [Column("expired_date")]
        public DateOnly ExpiredDate { get; set; }

        [Column("budget")]
        public decimal Budget { get; set; }

        // Navigation
        [ForeignKey("ProjectRequestId")]
        [InverseProperty("Contract")]
        public virtual ProjectRequest? ProjectRequest { get; set; }

        [InverseProperty("Contract")]
        public virtual ICollection<ContractAppendix> ContractAppendices { get; set; } = new List<ContractAppendix>();

    }
}
