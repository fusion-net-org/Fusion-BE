using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.AITaskGenerate
{
    public sealed class AiMaterializeTasksRequestDto
    {
        public Guid ProjectId { get; set; }

        public Guid? DefaultSprintId { get; set; }

    
        public List<AiGeneratedTaskDraftDto> Tasks { get; set; } = new();
    }
}
