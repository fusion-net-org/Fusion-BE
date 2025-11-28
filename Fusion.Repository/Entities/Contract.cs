using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Enums;
using Microsoft.EntityFrameworkCore;

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

        [Column("attachment")]
        public string? Attachment { get; set; }

        [Column("effective_date")]
        public DateOnly? EffectiveDate { get; set; }

        [Column("expired_date")]
        public DateOnly? ExpiredDate { get; set; }

        [Column("budget")]
        public decimal? Budget { get; set; }

        [Column("status")]
        [StringLength(20)]
        public string? Status { get; set; }

        [Column("reason")]
        public string? Reason { get; set; }

        [Column("created_by")]
        public Guid? CreatedBy { get; set; }

        [Column("create_at")]
        [Precision(3)]
        public DateTime CreateAt { get; set; }

        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        [Column("update_at")]
        [Precision(3)]
        public DateTime UpdateAt { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByNavigation { get; set; }

        [ForeignKey("UpdatedBy")]
        public virtual User? UpdatedByNavigation { get; set; }

        [Column("requester_company_id")]
        public Guid? RequesterCompanyId { get; set; }

        [Column("executor_company_id")]
        public Guid? ExecutorCompanyId { get; set; }

        [InverseProperty("Contract")]
        public virtual ProjectRequest? ProjectRequest { get; set; }

        [InverseProperty("Contract")]
        public virtual ICollection<ContractAppendix> ContractAppendices { get; set; } = new List<ContractAppendix>();

        [ForeignKey("RequesterCompanyId")]
        public virtual Company? RequesterCompany { get; set; }

        [ForeignKey("ExecutorCompanyId")]
        public virtual Company? ExecutorCompany { get; set; }


    }
}
