using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace Diploma.Models.DTOs
{
    public class DefectCreateDto
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        [Display(Name = "Defect Title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please select priority")]
        [Display(Name = "Priority")]
        public DefectPriority Priority { get; set; } = DefectPriority.Medium;

        [Required(ErrorMessage = "Premise is required")]
        [Display(Name = "Premise")]
        public int PremisesId { get; set; }

        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Photos")]
        public List<HttpPostedFileBase> Photos { get; set; }
    }

    public class DefectDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DefectStatus Status { get; set; }
        public DefectPriority Priority { get; set; }

        public int PremisesId { get; set; }
        public string PremiseNumber { get; set; }
        public int Floor { get; set; }
        public string ObjectName { get; set; }

        public string CreatedByUserName { get; set; }
        public string AssignedToUserName { get; set; }
        public string ContractorCompanyName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? AcceptedByOwnerAt { get; set; }

        public List<DefectCommentDto> Comments { get; set; }
        public List<DefectMediaDto> MediaFiles { get; set; }
        public List<DefectStatusHistoryDto> StatusHistory { get; set; }
    }

    public class DefectCommentDto
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DefectMediaDto
    {
        public int Id { get; set; }
        public string OriginalFileName { get; set; }
        public string StoredFileName { get; set; }
        public string ContentType { get; set; }
        public bool IsBeforeFix { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class DefectStatusHistoryDto
    {
        public int Id { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public string ChangedByUserName { get; set; }
        public DateTime ChangedAt { get; set; }
        public string Comment { get; set; }
    }

    public class DefectListDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DefectStatus Status { get; set; }
        public DefectPriority Priority { get; set; }
        public string PremiseNumber { get; set; }
        public int Floor { get; set; }
        public string ObjectName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public int CommentCount { get; set; }
        public int MediaCount { get; set; }
    }
}