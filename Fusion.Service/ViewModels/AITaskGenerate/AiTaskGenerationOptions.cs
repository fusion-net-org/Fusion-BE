using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.ViewModels.AITaskGenerate
{
    public sealed class AiTaskGenerationOptions
    {
        /// <summary>
        /// OpenAI / provider model name. E.g. "gpt-4.1-mini"
        /// </summary>
        public string Model { get; set; } = "gpt-4.1-mini";

        /// <summary>
        /// Base system prompt. If empty, service will use its internal default.
        /// </summary>
        public string? SystemPrompt { get; set; }

        /// <summary>
        /// Temperature for the model (0–2).
        /// </summary>
        public double Temperature { get; set; } = 0.2;

        /// <summary>
        /// Max tokens of output for the completion.
        /// </summary>
        public int MaxTokens { get; set; } = 2048;
    }
}
